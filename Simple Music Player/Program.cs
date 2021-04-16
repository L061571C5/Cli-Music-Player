using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;
using Id3;
namespace Simple_Music_Player
{

    class Program
    {
        static void Main(string[] args)
        {
            mainProgram(args);
        }
        static void mainProgram(string[] args)
        {
            
            string directory;
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("[1] Play Directory");
            Console.WriteLine("[2] Play Directory Shuffled");
            Console.WriteLine("[3] Play a file");
            string selection = Console.ReadLine();
            switch (selection[0])
            {
                case '1':
                    Console.WriteLine("What directory has music files?");
                    directory = Console.ReadLine();
                    if (dirChk(directory, false) == "null")
                    {
                        Main(args);
                        return;
                    }
                    MusicData.queue = ProcessDirectory(directory);
                    playMusic(args);
                    break;
                case '2':
                    Console.WriteLine("What directory has music files?");
                    directory = Console.ReadLine();
                    if (dirChk(directory, false) == "null")
                    {
                        Main(args);
                        return;
                    }
                    MusicData.queue = ProcessDirectory(directory).OrderBy(x => Guid.NewGuid()).ToList();
                    playMusic(args);
                    break;
                case '3':
                    Console.WriteLine("What file do you want to play?");
                    directory = Console.ReadLine();
                    dirChk(directory, true);
                    MusicData.queue.Add(directory);
                    playMusic(args);
                    break;
                default:
                    Console.WriteLine("Select a valid input.");
                    Main(args);
                    break;
            }
        }
        public static List<string> ProcessDirectory(string targetDirectory)
        {
            List<string> lst = new List<string>();
            dirChk(targetDirectory, false);
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(targetDirectory, "*.mp3", SearchOption.AllDirectories).Union(Directory.GetFiles(targetDirectory, "*.flac", SearchOption.AllDirectories));
            foreach (string fileName in fileEntries)
                lst.Add(fileName);
            return lst;
        }
        static string dirChk(string dir, bool file)
        {
            switch (file)
            {
                case true:
                    if (!File.Exists(dir))
                    {
                        Console.WriteLine("That is not a vaild file.");
                        return "null";
                    }
                    break;
                case false:
                    if (!Directory.Exists(dir))
                    {
                        Console.WriteLine("That is not a vaild directory.");
                        return "null";
                    }
                    break;
            }
               
            return dir;
        }
        public static void playMusic(string[] args)
        {
            using (var audioFile = new AudioFileReader(MusicData.queue[0]))
            using (var outputDevice = new WaveOut())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                    using (var mp3 = new Mp3(MusicData.queue[0]))
                    {
                        Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                        Console.Clear();
                        Console.WriteLine("Title: {0}", tag.Title);
                        Console.WriteLine("Artist: {0}", tag.Artists);
                        Console.WriteLine("Album: {0}", tag.Album);
                    }
                    double ms = outputDevice.GetPosition() * 1000.0 / audioFile.WaveFormat.BitsPerSample / audioFile.WaveFormat.Channels * 8 / audioFile.WaveFormat.SampleRate;
            TimeSpan ts = TimeSpan.FromMilliseconds(ms);
                    Console.WriteLine(ts.ToString(@"hh\:mm\:ss"));
                }
                MusicData.queue.RemoveAt(0);
                if (MusicData.queue.Count() >= 1)
                {
                    playMusic(args);
                        return;
                }
            }
            mainProgram(args);
        }
    }
}
