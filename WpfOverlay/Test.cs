using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Media;
using System.Diagnostics;

namespace WpfOverlay
{
     class Test 
    {
        private static bool Authentication = false;
        private static string CommandBuffer;
        public static Choices c = new Choices();
        public static Choices ca = new Choices();
        public static Choices cab = new Choices();
        public static SpeechRecognitionEngine rec;
        public static SoundPlayer player = new SoundPlayer("Confirm.wav");
        public static bool lockOverlay = false;
        private static float ConfidenceMin = 0.85f;
        private static bool Configure = false;
        private static bool configureP2 = false;
        private static float ConfidenceHolder;
        private static bool safety = false;
        public static void initializeVrec()
        {
            
          
          
            foreach(Command COmmand in MainWindow.Command)
            {
                if (COmmand.VoiceTrigger != null && COmmand.VoiceTrigger != "")
                {
                    c.Add(COmmand.VoiceTrigger);
                }
            }
            c.Add("Lock voice recognition");
            c.Add("unlock voice recognition");
            c.Add("Calibrate voice recognition");

            ca.Add("The quick brown fox jumps over the lazy dog");
            ca.Add("Short sentence");
            ca.Add("allow command");
            ca.Add("deny command");
         //   cab.Add("search google for");

            GrammarBuilder gb = GrammarBuilder.Add((GrammarBuilder)"overlay", c);
            GrammarBuilder gba = new GrammarBuilder(ca);
           // GrammarBuilder Dynamic = new GrammarBuilder("overlay");
           // Dynamic.Append(cab);
           // Dynamic.AppendDictation();
            Choices bothChoices = new Choices(new GrammarBuilder[] { gb, gba });

            Console.WriteLine(gb.DebugShowPhrases);
            //  gb.Append("overlay");
            Grammar g = new Grammar(bothChoices);



            rec = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            rec.SetInputToDefaultAudioDevice();
            rec.LoadGrammar(g);
            rec.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec_SpeechRecognized);
            rec.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(Speech_detected);
            rec.RecognizeAsync(RecognizeMode.Multiple);
            /*while (true)
            {
                Console.WriteLine(rec.Recognize().Text);
            }*/
        }

        private static void Speech_detected(object sender, SpeechDetectedEventArgs e)
        {
         //   MainWindow.synth.Speak("speech detected");
        }
        public static string ParseString(string str,string ToReplace)//returns a string parsed out with ToReplace
        {

            string NewS = str.Replace(ToReplace, "");
            return NewS;
        }


        private static void rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (Authentication == true)
            {
                
                if (e.Result.Confidence > ConfidenceMin)
                {
                    if (e.Result.Text == "allow command")
                    {
                        if (player != null)
                        {
                            player.Play();
                            MainWindow.synth.SpeakAsyncCancelAll();
                        }
                        Ccompiler.Compile(CommandBuffer);
                        Authentication = false;
                    }
                    else if (e.Result.Text == "deny command")
                    {
                        Authentication = false;
                        MainWindow.synth.Speak("ok");
                        return;
                    }
                }
            }
            //If authentication is needed all other commands but yes/no are disabled untill confirmation
            else
            {
               
                if (lockOverlay == false)
                {
                   

                        if (e.Result.Text == "overlay Calibrate voice recognition")
                        {
                            Configure = true;
                            Console.WriteLine("Starting Calibration");
                            if (player != null)
                            {
                                player.Play();
                            }

                        }

                        if (Configure == true)
                        {
                            if (configureP2 == false && safety == false)
                            {
                                MainWindow.synth.Speak("Repeat after me. The quick brown fox jumps over the lazy dog");
                                safety = true;
                            }
                            if (e.Result.Text == "The quick brown fox jumps over the lazy dog" && configureP2 == false)
                            {
                                ConfidenceHolder = e.Result.Confidence;
                                configureP2 = true;
                                MainWindow.synth.Speak("good, now say , short sentence");
                            }
                            else if (e.Result.Text == "Short sentence" && configureP2 == true)
                            {
                                ConfidenceHolder = e.Result.Confidence;
                                configureP2 = false;
                                Configure = false;
                                safety = false;
                                MainWindow.synth.Speak("Got it, Calibrating");
                                float Confidence2 = e.Result.Confidence;

                                ConfidenceMin = ((ConfidenceHolder + Confidence2) / 2) - 0.1f;
                                Console.WriteLine("confidence Minimum is now" + ConfidenceMin);
                                return;
                            }
                        }
                    }
                }
                Console.WriteLine("confidence is :" + e.Result.Confidence);
                if (e.Result.Confidence > ConfidenceMin)
                {
                   

                    if (e.Result.Text == "overlay Lock voice recognition")
                    {
                        MainWindow.synth.Speak("Locking Voice Commands");
                        lockOverlay = true;
                        return;
                    }
                    else if (e.Result.Text == "overlay unlock voice recognition")
                    {
                        if (lockOverlay == true)
                        {
                            MainWindow.synth.Speak("unLocking Voice Commands");
                            lockOverlay = false;
                            return;

                    }
                    else
                        {
                            MainWindow.synth.Speak("Voice COmmands Already unlocked");
                            return;

                    }
                }


                    if(lockOverlay == true && e.Result.Text.Contains("overlay"))
                {
                    MainWindow.synth.Speak("VR Locked");
                    return;
                }

                if (lockOverlay == false)
                    {
                        Console.WriteLine(e.Result.Text);
                       

                        foreach (Command CO in MainWindow.Command)
                        {
                            string CommandAnswer = "overlay " + CO.VoiceTrigger;
                            if (CommandAnswer == e.Result.Text)
                            {

                                if (CO.RequireAuth == false)
                                {
                                    if (player != null)
                                    {
                                        player.Play();
                                    }
                                    //Then Run CO.CommandContent Through the compiler
                                    Ccompiler.Compile(CO.CommandContent);
                                }

                                else
                                {
                                    Authentication = true;
                                    CommandBuffer = CO.CommandContent;
                                    MainWindow.synth.Speak("Command " + CO.ButtonName + "needs authentication");
                                   
                                }
                                break;
                            }
                            
                        }

                    }
                }

            }
        
        }

    }


