using MizSuite.ConsoleApp;
using System;
using System.IO;
using System.Reflection;
using System.Text;


// See https://aka.ms/new-console-template for more information

string path = "c:\\temp";
//path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
string extractPath = $"{path}\\extract"; 
string processedPath = $"{path}\\processed";

Console.WriteLine("Looking for .MIZ files...");

if (!Directory.Exists(path))
    Directory.CreateDirectory(path);
if(!Directory.Exists(extractPath))
    Directory.CreateDirectory(extractPath);
if (!Directory.Exists(processedPath))
    Directory.CreateDirectory(processedPath);

var filesInDirectory = Directory.GetFiles(path).ToList();
var mizFiles = filesInDirectory.Where(f => f.EndsWith(".miz")).ToList();

if (mizFiles.Any())
{
    foreach(var mizFile in mizFiles)
    {
        Console.WriteLine($"Found \u001b[36m{Path.GetFileName(mizFile)}\u001b[0m");
    }
}

foreach (var mizFile in mizFiles)
{
    Console.Write($"Do you want to process \u001b[36m{Path.GetFileName(mizFile)}\u001b[0m? N/Y[Default]: ");
    var answer = Console.ReadLine().ToUpper();
    if (answer is not null && answer == "N")
        continue;

    //cleanup files in extract directory
    var filesToDelete = Directory.EnumerateFiles(extractPath);
    foreach (var f in filesToDelete)
    {
        File.Delete(f);
    }

    //cleanup files in processed directory
    filesToDelete = Directory.EnumerateFiles(processedPath);
    foreach (var f in filesToDelete)
    {
        File.Delete(f);
    }

    //cleanup directories in extract directory
    var directoriesToDelete = Directory.EnumerateDirectories(extractPath);
    foreach(var d in directoriesToDelete)
    {
        Directory.Delete(d,true);
    }

    System.IO.Compression.ZipFile.ExtractToDirectory(mizFile, extractPath);
    System.IO.Compression.ZipFile.CreateFromDirectory(extractPath, $"{processedPath}\\{Path.GetFileName(mizFile)}.miz");

    string[] file = File.ReadAllLines($"{extractPath}\\mission");
    List<LineClass> lines = LineClass.CreateList(file);
    List<LineClass> radiosBegin = lines.Where(x => x.Content.Contains("[\"Radio\"] = ")).OrderBy(x => x.Index).ToList();
    List<LineClass> radiosEnd = lines.Where(x => x.Content.Contains("-- end of [\"Radio\"]")).OrderBy(x => x.Index).ToList();

    List<Airframe> airframe = new List<Airframe>();

    for (int i = 0; i < radiosBegin.Count; i++)
    {
        airframe.Add(new Airframe(radiosBegin[i], radiosEnd[i], ref lines));
    }

    for(int i = 0; i < airframe.Count; i++)
    {
        Console.WriteLine("===============================================");
        Console.WriteLine($"{airframe[i].Type}");
        foreach(Radio radio in airframe[i].Radios)
        {
            Console.WriteLine($"\tRadio: {radio.Number}");
            foreach(Channel channel in radio.Channels.OrderBy(c => c.Number))
            {
                Console.WriteLine($"\t\tChannel {channel.Number} - {channel.Frequency}");
            }
        }
        Console.WriteLine("===============================================");
    }

    var test = 0;

}

