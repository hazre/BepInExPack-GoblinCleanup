using Mono.Cecil;

public static class PrepareUnstrippedCorlib
{
    public static AssemblyDefinition? SafeReadAssembly(string path, ReaderParameters rp)
    {
        try { return AssemblyDefinition.ReadAssembly(path, rp); }
        catch { return null; }
    }

    public static (int types, int methods) CountMembers(AssemblyDefinition asm)
    {
        int types = 0, methods = 0;

        void Walk(TypeDefinition t)
        {
            types++;
            methods += t.Methods.Count;
            foreach (var n in t.NestedTypes)
                Walk(n);
        }

        foreach (var mod in asm.Modules)
            foreach (var type in mod.Types)
                Walk(type);

        return (types, methods);
    }

    public static void Run(ICakeContext ctx, string unityVersion, string unityHub, string gamePath)
    {
        var nonDevDir = $@"{unityHub}\{unityVersion}\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_player_nondevelopment_mono\Data\Managed";
        var jitDir = $@"{unityHub}\{unityVersion}\Editor\Data\MonoBleedingEdge\lib\mono\unityjit-win32";
        var facadeDir = $@"{jitDir}\Facades";
        var gameDir = $@"{gamePath}\{System.IO.Path.GetFileNameWithoutExtension(gamePath)}_Data\Managed";
        var targetDir = System.IO.Path.GetFullPath("./UnstrippedCorlib");

        ctx.Information($"Unity version: {unityVersion}");
        ctx.Information($"Non-dev player: {nonDevDir}");
        ctx.Information($"UnityJit: {jitDir}");
        ctx.Information($"Game Managed: {gameDir}");
        ctx.Information($"Target: {targetDir}");

        var errors = new System.Collections.Generic.List<string>();
        var unityErrors = false;
        if (!System.IO.Directory.Exists(unityHub))
        { errors.Add($"UnityHubDir not found: {unityHub}"); unityErrors = true; }
        var unityVerDir = $@"{unityHub}\{unityVersion}";
        if (!System.IO.Directory.Exists(unityVerDir))
        { errors.Add($"Unity version directory not found: {unityVerDir}"); unityErrors = true; }
        if (!System.IO.Directory.Exists(nonDevDir))
        { errors.Add($"Non-dev player directory not found: {nonDevDir}"); unityErrors = true; }
        if (!System.IO.Directory.Exists(jitDir))
        { errors.Add($"UnityJit directory not found: {jitDir}"); unityErrors = true; }
        var gameErrors = false;
        if (!System.IO.Directory.Exists(gameDir))
        { errors.Add($"Game Managed directory not found: {gameDir}"); gameErrors = true; }
        if (errors.Count > 0)
        {
            var msg = "Required directories not found:\n  " + string.Join("\n  ", errors);
            if (unityErrors)
                msg += $"\n\nPlease install Unity {unityVersion} from:\n  https://unity.com/releases/editor/archive";
            if (gameErrors)
                msg += "\n\nPlease install Goblin Cleanup via Steam:\n  https://store.steampowered.com/app/2748340/Goblin_Cleanup";
            throw new System.Exception(msg);
        }

        var targetPath = new DirectoryPath(targetDir);
        if (ctx.DirectoryExists(targetPath)) ctx.CleanDirectory(targetPath);
        else ctx.CreateDirectory(targetPath);

        var rp = new ReaderParameters { ReadSymbols = false, InMemory = true };

        var ndFiles = new System.IO.DirectoryInfo(nonDevDir).GetFiles("UnityEngine*.dll")
            .ToDictionary(f => f.Name, f => f);
        var gameFiles = new System.IO.DirectoryInfo(gameDir).GetFiles("UnityEngine*.dll")
            .ToDictionary(f => f.Name, f => f);

        var inGameNames = gameFiles.Keys.Intersect(ndFiles.Keys).ToHashSet();
        var neededExtras = new HashSet<string>();

        foreach (var name in inGameNames)
        {
            using var asm = SafeReadAssembly(ndFiles[name].FullName, rp);
            if (asm == null) continue;

            foreach (var aref in asm.Modules[0].AssemblyReferences)
            {
                if (aref.Name.StartsWith("UnityEngine") || aref.Name.StartsWith("Unity."))
                {
                    var dllName = aref.Name + ".dll";
                    if (!inGameNames.Contains(dllName) && ndFiles.ContainsKey(dllName))
                        neededExtras.Add(dllName);
                }
            }
        }

        var queue = new Queue<string>(neededExtras);
        while (queue.Count > 0)
        {
            var extra = queue.Dequeue();
            using var asm = SafeReadAssembly(ndFiles[extra].FullName, rp);
            if (asm == null) continue;

            foreach (var aref in asm.Modules[0].AssemblyReferences)
            {
                var dllName = aref.Name + ".dll";
                if (dllName.StartsWith("UnityEngine") && !inGameNames.Contains(dllName)
                    && !neededExtras.Contains(dllName) && ndFiles.ContainsKey(dllName))
                {
                    neededExtras.Add(dllName);
                    queue.Enqueue(dllName);
                }
            }
        }

        foreach (var name in inGameNames)
            ndFiles[name].CopyTo(System.IO.Path.Combine(targetDir, name), overwrite: true);

        foreach (var name in neededExtras)
            ndFiles[name].CopyTo(System.IO.Path.Combine(targetDir, name), overwrite: true);

        var neededSystem = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<string>();

        void EnqueueRefs(AssemblyDefinition asm)
        {
            foreach (var aref in asm.Modules[0].AssemblyReferences)
            {
                var refName = aref.Name;
                if (refName.StartsWith("UnityEngine") || refName.StartsWith("Unity.")) continue;
                var dllName = refName + ".dll";
                if (neededSystem.Add(dllName))
                    pending.Enqueue(dllName);
            }
        }

        foreach (var name in inGameNames.Concat(neededExtras))
        {
            using var asm = SafeReadAssembly(ndFiles[name].FullName, rp);
            if (asm != null) EnqueueRefs(asm);
        }

        while (pending.Count > 0)
        {
            var dllName = pending.Dequeue();
            var jitSrc = System.IO.Path.Combine(jitDir, dllName);
            var facadeSrc = System.IO.Path.Combine(facadeDir, dllName);
            var srcPath = System.IO.File.Exists(jitSrc) ? jitSrc :
                          System.IO.File.Exists(facadeSrc) ? facadeSrc : null;
            if (srcPath == null) continue;

            var gameSrc = System.IO.Path.Combine(gameDir, dllName);
            var gameExists = System.IO.File.Exists(gameSrc);
            var gameSize = gameExists ? new System.IO.FileInfo(gameSrc).Length : 0L;
            var jitSize = new System.IO.FileInfo(srcPath).Length;

            if (!gameExists || gameSize != jitSize)
            {
                System.IO.File.Copy(srcPath, System.IO.Path.Combine(targetDir, dllName), overwrite: true);

                using var refAsm = SafeReadAssembly(srcPath, rp);
                if (refAsm != null) EnqueueRefs(refAsm);
            }
        }

        var total = new System.IO.DirectoryInfo(targetDir).GetFiles("*.dll").Length;
        ctx.Information($"\nUnstrippedCorlib: {total} DLLs");

        ctx.Information("\nUnity module comparison (game vs non-dev player):");
        var tableFmt = $"  {{0,-60}} {{1,6}} {{2,6}} {{3,7}} {{4,7}} {{5,7}} {{6,8}} {{7,8}} {{8,8}} {{9}}";
        ctx.Information(string.Format(tableFmt, "Module", "Game", "NonD", "GameTy", "NonDTy", "DiffTy", "GameMet", "NonDMet", "DiffMet", "Status"));
        ctx.Information(new string('-', 130));

        foreach (var f in ndFiles.Values.OrderBy(f => f.Name))
        {
            var name = f.Name;

            if (!gameFiles.TryGetValue(name, out var gf))
            {
                var reason = neededExtras.Contains(name) ? "referenced" : "unreferenced";
                ctx.Information(string.Format(tableFmt, name, "", "", "", "", "", "", "", "", $"[not in game, {reason}]"));
                continue;
            }

            using var ndAsm = SafeReadAssembly(f.FullName, rp);
            using var gmAsm = SafeReadAssembly(gf.FullName, rp);

            if (ndAsm == null || gmAsm == null)
            {
                ctx.Information(string.Format(tableFmt, name, "", "", "", "", "", "", "", "", "ERROR (failed to load)"));
                continue;
            }

            var nd = CountMembers(ndAsm);
            var gm = CountMembers(gmAsm);
            var diffTypes = nd.types - gm.types;
            var diffMethods = nd.methods - gm.methods;

            string status;
            if (diffTypes == 0 && diffMethods == 0)
                status = "SAME";
            else if (diffTypes < 0 || diffMethods < 0)
                status = "UNKNOWN";
            else
                status = "STRIPPED";

            ctx.Information(string.Format(tableFmt,
                name,
                $"{gf.Length / 1024:F1}",
                $"{f.Length / 1024:F1}",
                gm.types,
                nd.types,
                diffTypes,
                gm.methods,
                nd.methods,
                diffMethods,
                status));
        }
    }
}
