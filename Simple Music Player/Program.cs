using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;

namespace Simple_Music_Player
{

    class Program
    {
        public static ISoundOut soundOut;
        public static IWaveSource waveSource;

        static void Main(string[] args)
        {
            Console.Title = "Simple Music Player";
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
            var fileEntries = Directory.GetFiles(targetDirectory, "*.mp3", SearchOption.AllDirectories).Union(Directory.GetFiles(targetDirectory, "*.flac", SearchOption.AllDirectories)).Union(Directory.GetFiles(targetDirectory, "*.ogg", SearchOption.AllDirectories));
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
            try
            {
                Console.Clear();
                var i = 5;
                //initialize file and play
                waveSource = CodecFactory.Instance.GetCodec(MusicData.queue[0]).ToSampleSource().ToWaveSource();
                soundOut = new WasapiOut() { Latency = 100 };
                soundOut.Initialize(waveSource);
                soundOut.Play();
                soundOut.Volume = MusicData.volume;

                //get the tags
                TagLib.File file = TagLib.File.Create(MusicData.queue[0]);
                TimeSpan time = waveSource.GetTime(waveSource.Length);
                var title = String.IsNullOrWhiteSpace(file.Tag.Title) ? file.Name.Split("\\")[file.Name.Split("\\").Length - 1].Split(".")[0] : file.Tag.Title;
                var artist = file.Tag.Performers.Length == 0 ? "N/A" : file.Tag.Performers.Length > 1 ? String.Join(", ", file.Tag.Performers) : file.Tag.Performers[0];
                var album = String.IsNullOrWhiteSpace(file.Tag.Album) ? "N/A" : file.Tag.Album;
                Console.WriteLine("Title: " + title);
                Console.WriteLine("Artist: " + artist);
                Console.WriteLine("Album: " + album);
                RewriteLine(5, "Type \"help\" for a commands list");

                //commands while playing 
                while (soundOut.PlaybackState == PlaybackState.Playing || soundOut.PlaybackState == PlaybackState.Paused)
                {
                    Console.SetCursorPosition(0, i);
                    i += 2;
                    while (true)
                    {
                        //get the time
                        double ms = waveSource.Position * 1000.0 / waveSource.WaveFormat.BitsPerSample / waveSource.WaveFormat.Channels * 8 / waveSource.WaveFormat.SampleRate;
                        TimeSpan ts = TimeSpan.FromMilliseconds(ms);
                        if (Console.KeyAvailable)
                        {
                            var input = Console.ReadLine();
                            switch (input)
                            {
                                case "help":
                                    Console.WriteLine("Commands:");
                                    Console.WriteLine("\"help\": Shows this menu");
                                    Console.WriteLine("\"resume\": Resumes curent song");
                                    Console.WriteLine("\"pause\": Pauses current song");
                                    Console.WriteLine("\"skip\": Skips current song");
                                    Console.WriteLine("\"volume\": View current volume or set the volume to a number from 0-100");
                                    Console.WriteLine("\"clear\": Clears the console");
                                    Console.WriteLine("\"stop\": Stops the application");
                                    i += 6;
                                    break;
                                case "resume":
                                    if (soundOut.PlaybackState != PlaybackState.Playing)
                                    {
                                        soundOut.Play();
                                        Console.WriteLine("Unpaused \"{0}\"", file.Tag.Title);
                                    }
                                    else
                                    {
                                        Console.WriteLine("The song is already playing");
                                    }
                                    break;
                                case "pause":
                                    if (soundOut.PlaybackState != PlaybackState.Paused)
                                    {
                                        soundOut.Pause();
                                        Console.WriteLine("Paused \"{0}\"", file.Tag.Title);
                                    }
                                    else
                                    {
                                        Console.WriteLine("The song is already paused");
                                    }
                                    break;
                                case "skip":
                                    soundOut.Stop();
                                    CleanupPlayback();
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
                                    MusicData.queue.RemoveRange(0, MusicData.queue.Count);
                                    soundOut.Stop();
                                    CleanupPlayback();
                                    Main(args);
                                    break;
                                case "volume":
                                    bool did = false;
                                    Console.WriteLine("The current volume is at {0}%", Math.Round(soundOut.Volume * 100));
                                    Console.WriteLine("Do you want to change the volume? (y/n)");
                                    while (!did)
                                    {
                                        var response = Console.ReadLine().ToLower();
                                        if (response == "y")
                                        {
                                            float number;
                                            Console.WriteLine("What volume would you like to change it to?");
                                            var value = Console.ReadLine();
                                            bool success = float.TryParse(value, out number);
                                            if (success)
                                            {
                                                if (number > 100f || number < 0f)
                                                {
                                                    Console.WriteLine("Volume must be between 0 and 100");
                                                    break;
                                                }
                                                number /= 100;
                                                soundOut.Volume = number;
                                                MusicData.volume = number;
                                                Console.WriteLine("Volume changed to {0}%", Math.Round(soundOut.Volume * 100));
                                            }
                                            else
                                            {
                                                Console.WriteLine("\"{0}\" is not a valid number between 0 and 100", value);
                                            }
                                            did = true;
                                            break;
                                        }
                                        else if (response == "n")
                                        {
                                            did = true;
                                            break;
                                        }
                                        Console.WriteLine("Choose yes (y) or no (n)");
                                    }
                                    break;
                                default:
                                    Console.WriteLine("Type \"help\" for a commands list");
                                    break;
                            }
                        }
                        RewriteLine(4, ts.ToString(@"hh\:mm\:ss") + " \\ " + time.ToString(@"hh\:mm\:ss"));
                        if (soundOut.PlaybackState == PlaybackState.Stopped)
                        {
                            MusicData.queue.RemoveAt(0);
                            if (MusicData.queue.Count() >= 1)
                            {
                                CleanupPlayback();
                                playMusic(args);
                                return;
                            }
                            else mainProgram(args);
                        }
                        Thread.Sleep(1000);
                    }
                }
            }
            catch
            {
                Console.WriteLine("An error occured, the file that this error occured on is {0} or {1}", MusicData.queue[0], MusicData.queue.Count >= 2 ? MusicData.queue[1] : "N/A");
            }
        }
        public static void RewriteLine(int lineNumber, String newText)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, lineNumber - 1);
            Console.Write(newText); Console.WriteLine(new string(' ', Console.WindowWidth - newText.Length));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        public static void CleanupPlayback()
        {
            soundOut.Dispose();
            soundOut = null;
            waveSource.Dispose();
            waveSource = null;

        }
    }
    static class MusicData
    {
        public static List<string> queue = new List<string>();
        public static float volume = 1f;
    }

}
