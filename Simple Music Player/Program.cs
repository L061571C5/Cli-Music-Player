using System;
using System.IO;
using System.Collections;
using System.Configuration;
using System.Linq
namespace Simple_Music_Player
{

    class Program
    {
        MusicData musicData = new MusicData();

        static void Main(string[] args)
        {
            mainProgram(args);
        }
        static void mainProgram(string[] args)
        {
            string directory;
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("[1] Shuffle Directory");
            Console.WriteLine("[2] Play a file");
            string selection = Console.ReadLine();
            switch (selection[0])
            {
                case '1':
                    Console.WriteLine("What directory has music files?");
                    directory = Console.ReadLine();
                    if (dirChk(directory) == "null")
                    {
                        Main(args);
                        return;
                    }

                    break;
                case '2':
                    Console.WriteLine("What file do you want to play?");
                    directory = Console.ReadLine();
                    dirChk(directory);
                    break;
                default:
                    Console.WriteLine("Select a valid input.");
                    Main(args);
                    break;
            }
        }
        public static string[] ProcessDirectory(string targetDirectory, )
        {
            string[] queuearr;
            var tempList = queuearr.ToList();
            dirChk(targetDirectory);
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                
            
            
                

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }
        static string dirChk(string dir)
        {
            if (!System.IO.Directory.Exists(dir))
            {
                Console.WriteLine("That is not a vaild directory/file.");
                return "null";
            }
            return dir;
        }
    }
}
