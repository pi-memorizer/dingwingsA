using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Threading;

#if WINDOWS
using NAudio.Wave;
#endif

namespace Hardware
{
#if WINDOWS
    class Sound
    {
        public static string baseSong = "Content/music/Bongo";
        public static string shopSong = "Content/music/shop.wav";

        public static SoundEffect clap, menu, die, money, success, trampoline, dash, dashPad, jump, victory, owenWilson;
        public static SoundEffect startVoiceover, middleVoiceover, endVoiceover;

        static WaveOutEvent songInstance = new WaveOutEvent();
        static string currentTheme = "";
        static LoopStream loopStream;
        static SoundEffectInstance introInstance;
        static bool changedThisFrame = false;

        public static void init()
        {
            loopStream = new LoopStream(new AudioFileReader(baseSong+"0.wav"));
            songInstance.Init(loopStream);
            songInstance.Stop();
            setMusic(baseSong);
        }
        public static void update()
        {
            changedThisFrame = false;
        }


        //TODO make soundeffect loading same in all builds
        //TODO make looping smoother
        //TODO figure out way to make a song give up loading to start a new thing
        public static void loadContent(ContentManager content)
        {
            clap = content.Load<SoundEffect>("SFX/clap");
            die = content.Load<SoundEffect>("SFX/die");
            menu = content.Load<SoundEffect>("SFX/MenuSFX");
            money = content.Load<SoundEffect>("SFX/money");
            success = content.Load<SoundEffect>("SFX/success");
            trampoline = content.Load<SoundEffect>("SFX/trampoline");
            dash = content.Load<SoundEffect>("SFX/dash");
            dashPad = content.Load<SoundEffect>("SFX/dashPad");
            jump = content.Load<SoundEffect>("SFX/jump");
            victory = content.Load<SoundEffect>("SFX/victory");
            owenWilson = content.Load<SoundEffect>("SFX/OwenWilson");
            startVoiceover = content.Load<SoundEffect>("voiceover/introVoiceover");
            middleVoiceover = content.Load<SoundEffect>("voiceover/middleVoiceover");
            endVoiceover = content.Load<SoundEffect>("voiceover/endVoiceover");
        }
        
        public static void unloadContent()
        {
            songInstance.Dispose();
        }

        public static void setMusic(string song, SoundEffect intro)
        {
            if(song!=shopSong) song += Core.getMusicLevel() + ".wav";
            if (changedThisFrame) return;
            if (currentTheme == song) {
                if(songInstance.PlaybackState!=PlaybackState.Playing)
                {
                    songInstance.Play();
                }
                return;
            }
            introInstance = intro.CreateInstance();
            introInstance.Play();
            currentTheme = song;
            changedThisFrame = true;
            var t = new Thread(setMusicIntroAsync);
            t.Start(song);
        }

        public static void setMusic(string song)
        {
            if(song!=shopSong) song += Core.getMusicLevel()+".wav";
            if (changedThisFrame) return;
            if (currentTheme == song)
            {
                if (songInstance.PlaybackState != PlaybackState.Playing)
                {
                    songInstance.Play();
                }
                return;
            }
            currentTheme = song;
            changedThisFrame = true;
            songInstance.Stop();
            loopStream.changeSource(song);
            songInstance.Play();
        }
        
        private static void setMusicIntroAsync(object o)
        {
            string song = o as string;

            if (songInstance.PlaybackState == PlaybackState.Playing) songInstance.Stop();
            loopStream.changeSource(song);

            while(introInstance.State==SoundState.Playing)
            {
                Thread.Sleep(1);
            }

            songInstance.Play();
        }

        public static void stopMusic()
        {
            songInstance.Stop();
        }
    }

    public class LoopStream : WaveStream
    {
        WaveStream sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length
        {
            get { return sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public void changeSource(string path)
        {
            sourceStream.Dispose();
            sourceStream = new AudioFileReader(path);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
#else
    class Sound
    {
        public static SoundEffect wildBattleIntro;
        public static Song cloudlandMusic, wildBattle;

        enum IntroMusicState { Started, IntroPlaying, ThemePlaying }
        static IntroMusicState introMusicState = IntroMusicState.ThemePlaying;
        static SoundEffectInstance introInstance;
        static Song currentTheme;

        public static void init()
        {

        }

        public static void update()
        {
            switch (introMusicState)
            {
                case IntroMusicState.Started:
                    if(MediaPlayer.State==MediaState.Playing&&MediaPlayer.PlayPosition.TotalMilliseconds>0)
                    {
                        //MediaPlayer.Pause();
                        introMusicState = IntroMusicState.IntroPlaying;
                    }
                    break;
                case IntroMusicState.IntroPlaying:
                    if (introInstance.State == SoundState.Stopped)
                    {
                        //if (MediaPlayer.State == MediaState.Paused)
                        {
                            //MediaPlayer.Resume();
                            Debug.Assert(MediaPlayer.Volume == 1);
                            MediaPlayer.Volume = 1;
                            introMusicState = IntroMusicState.ThemePlaying;
                        }
                    }
                    break;
                case IntroMusicState.ThemePlaying:
                    break;
            }
        }

        public static void loadContent(ContentManager content)
        {
            cloudlandMusic = content.Load<Song>("music/nostalgic_cumulus");
            wildBattle = content.Load<Song>("music/wild_battle");
            wildBattleIntro = content.Load<SoundEffect>("music/messan_wild_intro");
        }
        
        public static void setMusic(Song clip)
        {
            //TODO
        }

        public static void setMusic(Song clip, SoundEffect intro)
        {
            if (currentTheme == clip) return;
            Debug.WriteLine(clip.Name);
            currentTheme = clip;
            MediaPlayer.Stop();
            Debug.Assert(MediaPlayer.State == MediaState.Stopped);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(clip);
            Debug.Assert(MediaPlayer.State == MediaState.Playing);
            //MediaPlayer.Pause();
            introInstance = intro.CreateInstance();
            introInstance.IsLooped = false;
            introInstance.Play();
            introMusicState = IntroMusicState.Started;
        }

        public static void setMusicInWorld(Song clip)
        {
            //TODO
            if (currentTheme == clip||introMusicState!=IntroMusicState.ThemePlaying) return;
            Debug.WriteLine(clip.Name);
            currentTheme = clip;
            MediaPlayer.Stop();
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(clip);
        }
    }
#endif
}
