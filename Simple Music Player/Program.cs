using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using System.Threading;
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
                    Console.WriteLine("What directory has music files? (type cancel to return)");
                    directory = Console.ReadLine();
                    if (directory == "cancel")
                    {
                        Main(args);
                        return;
                    }
                    if (dirChk(directory, false) == "null")
                    {
                        Main(args);
                        return;
                    }
                    MusicData.queue = ProcessDirectory(directory);
                    playMusic(args);
                    break;
                case '2':
                    Console.WriteLine("What directory has music files? (type cancel to return)");
                    directory = Console.ReadLine();
                    if (directory == "cancel")
                    {
                        Main(args);
                        return;
                    }
                    if (dirChk(directory, false) == "null")
                    {
                        Main(args);
                        return;
                    }
                    MusicData.queue = (ProcessDirectory(directory)).OrderBy(x => Guid.NewGuid()).ToList();
                    playMusic(args);
                    break;
                case '3':
                    Console.WriteLine("What file do you want to play? (type cancel to return)");
                    directory = Console.ReadLine();
                    if (directory == "cancel")
                    {
                        Main(args);
                        return;
                    }
                    if (dirChk(directory, true) == "null")
                    {
                        Main(args);
                        return;
                    }
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
                    if (!System.IO.File.Exists(dir))
                    {
                        Console.WriteLine("That is not a valid file.");
                        return "null";
                    }
                    break;
                case false:
                    if (!Directory.Exists(dir))
                    {
                        Console.WriteLine("That is not a valid directory.");
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
                TagLib.File file = TagLib.File.Create(MusicData.queue[0]);
                Console.Clear();
                var i = 5;
                double ms;
                var time = audioFile.TotalTime;
                var artist = file.Tag.Performers.Length > 1 ? String.Join(", ", file.Tag.Performers) : file.Tag.Performers[0];
                Console.WriteLine("Title: " + file.Tag.Title);
                Console.WriteLine("Artist: " + artist);
                Console.WriteLine("Album: " + file.Tag.Album);
                RewriteLine(5, "Type \"help\" for a commands list");
                while (outputDevice.PlaybackState == PlaybackState.Playing || outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    Console.SetCursorPosition(0, i);
                    i += 2;
                    while (true)
                    {
                        ms = outputDevice.GetPosition() * 1000.0 / audioFile.WaveFormat.BitsPerSample / audioFile.WaveFormat.Channels * 8 / audioFile.WaveFormat.SampleRate;
                        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
                        if (Console.KeyAvailable)
                        {
                            var input = Console.ReadLine();
                            switch (input)
                            {
                                case "help":
                                    Console.WriteLine("Commands:");
                                    Console.WriteLine("\"help\": Shows this menu");
                                    Console.WriteLine("\"play\": Unpauses curent song");
                                    Console.WriteLine("\"pause\": Pauses current song");
                                    Console.WriteLine("\"skip\": Skips current song");
                                    Console.WriteLine("\"clear\": Clears the console");
                                    Console.WriteLine("\"stop\": Stops the application");
                                    i += 6;
                                    break;
                                case "play":
                                    if (outputDevice.PlaybackState != PlaybackState.Playing)
                                    {
                                        outputDevice.Play();
                                        Console.WriteLine("Unpaused \"{0}\"", file.Tag.Title);

                                    }
                                    else
                                    {
                                        Console.WriteLine("The song is already playing");
                                    }
                                    break;
                                case "pause":
                                    if (outputDevice.PlaybackState != PlaybackState.Paused)
                                    {
                                        outputDevice.Pause();
                                        Console.WriteLine("Paused \"{0}\"", file.Tag.Title);

                                    }
                                    else
                                    {
                                        Console.WriteLine("The song is already paused");
                                    }
                                    break;
                                case "skip":
                                    outputDevice.Stop();
                                    MusicData.queue.RemoveAt(0);
                                    if (MusicData.queue.Count() >= 1)
                                    {
                                        playMusic(args);
                                        return;
                                    }
                                    else mainProgram(args);
                                    break;
                                case "clear":
                                    Console.Clear();
                                    RewriteLine(1, "Title: " + file.Tag.Title);
                                    RewriteLine(2, "Artist: " + artist);
                                    RewriteLine(3, "Album: " + file.Tag.Album);
                                    RewriteLine(4, ts.ToString(@"hh\:mm\:ss") + " \\ " + time);
                                    RewriteLine(5, "Type \"help\" for a commands list");
                                    i = 5;
                                    Console.SetCursorPosition(0, i);
                                    break;
                                case "stop":
                                    Main(args);
                                    break;
                                default:
                                    Console.WriteLine("Type \"help\" for a commands list");
                                    break;
                            }
                        }
                        RewriteLine(4, ts.ToString(@"hh\:mm\:ss") + " \\ " + time);
                        if (outputDevice.PlaybackState == PlaybackState.Stopped)
                        {
                            MusicData.queue.RemoveAt(0);
                            if (MusicData.queue.Count() >= 1)
                            {
                                playMusic(args);
                                return;
                            }
                            else mainProgram(args);
                        }
                        Thread.Sleep(750);
                    }
                }
            }
        }
        public static void RewriteLine(int lineNumber, String newText)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, lineNumber - 1);
            Console.Write(newText); Console.WriteLine(new string(' ', Console.WindowWidth - newText.Length));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
    static class MusicData
    {
        public static List<string> queue = new List<string>();
    }

}
