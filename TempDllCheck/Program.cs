using System;
using System.IO;
using System.Reflection;

var dllPath = @"d:\unity\Restaurant\HybridCLRData\HotUpdateDlls\StandaloneWindows64\HotUpdate.dll";
var bytes = File.ReadAllBytes(dllPath);
var asm = Assembly.Load(bytes);
Console.WriteLine($"Assembly: {asm.FullName} Size: {bytes.Length}");

try
{
    var types = asm.GetTypes();
    foreach (var t in types)
        Console.WriteLine($"  {t.FullName}");
}
catch (ReflectionTypeLoadException ex)
{
    Console.WriteLine("Types found (before resolution errors):");
    foreach (var t in ex.Types)
        if (t != null) Console.WriteLine($"  {t.FullName}");
}

// Also check if HotUpdateEntry exists
bool found = false;
try { var types = asm.GetTypes(); found = Array.Exists(types, t => t.Name == "HotUpdateEntry"); }
catch (ReflectionTypeLoadException ex) { found = Array.Exists(ex.Types, t => t?.Name == "HotUpdateEntry"); }
Console.WriteLine($"\nHotUpdateEntry found: {found}");
