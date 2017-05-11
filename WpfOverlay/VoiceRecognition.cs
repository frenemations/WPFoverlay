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
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;

namespace WpfOverlay
{
    class VoiceRecognition
    {
        public static bool Shutup = false;//Prevent Overlay from prompting "vr Locked"
        public static int minimum = 0;
        public static List<GrammarBuilder> DynamicGbuilder = new List<GrammarBuilder>();
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

            try
            { 

            Parallel.ForEach(MainWindow.Command, (COmmand) =>
             {

                 COmmand.VoiceTrigStringName = new List<string>();//Safety 
                 COmmand.VoiceTrigStringNamePosition = new List<int>();
                 COmmand.VoiceTrigIntName = new List<string>();

                 List<Choices> ChoiceArr = new List<Choices>();//choices must be declared here to avoid the prallel foreach statement from messing up the voice commands

                 if (COmmand.VoiceTrigger != null && COmmand.VoiceTrigger != "")
                 {
                     string CommandV = ParseString(COmmand.VoiceTrigger, " ");//Remove Spaces 
                     CommandV = CommandV.ToLower();//To lowerCase

                     if (!CommandV.Contains("&") && !CommandV.Contains("|"))  // test &int/min=2/max=20/intname=banana& (song|songs) &now
                         c.Add(CommandV);
                     else
                     {

                         string[] CommandVoiceCollection = CommandV.Split('&');  //test    int/min=2/max=20/intname=banana     (song|songs)     now
                         int ForeachCounter = -1;

                         foreach (string S in CommandVoiceCollection)//Each of these needs to add to a choice or return the foreach
                         {
                             ForeachCounter += 1;//On which turn is the foreach?
                             if (S.Contains("int"))
                             {
                                 try
                                 {
                                     if (!S.Contains("intname="))
                                     {
                                         MessageBox.Show("Command " + COmmand.ButtonName + " Has an int argument but has no intname argument, This is required for the voicecommand to be able to trigger", "VoiceTrigger Failed loading Argumemts");
                                     }

                                     string[] SS = S.Split('/');//split Int identifier into its value catagories    
                                                                //   int   min=2   max=20  intname=banana
                                     foreach (string Value in SS)
                                     {
                                         //Remember to add choice once you get the maximum

                                         if (Value.Contains("min="))//checks for min catagory
                                         {

                                             string choiceToInt = ParseString(Value, "min=");

                                             if (!Int32.TryParse(choiceToInt, out minimum))//if no maximun is found minimun won't matter
                                             {
                                                 MessageBox.Show("Minimum Identifier found but no whole number found after identifier");
                                                 return;
                                             }

                                         }
                                         else if (Value.Contains("max="))//checks for max catagory
                                         {
                                             string choiceToInt = ParseString(Value, "max=");
                                             int maximum;
                                             if (!Int32.TryParse(choiceToInt, out maximum))
                                             {
                                                 MessageBox.Show("Maximun Identifier found but no whole number found after identifier");
                                                 return;
                                             }
                                             ChoiceArr.Add(new Choices(minimum.ToString()));
                                             //add the first one

                                             for (int a = minimum + 1; a <= maximum; a += 1)//for loop to add everything from minimum to maximum
                                             {
                                               //  Debug.WriteLine(a);
                                                 ChoiceArr[ChoiceArr.Count() - 1].Add(a.ToString());
                                             }



                                         }
                                         else// split / indicated but no detected value gotten
                                         {
                                             if (!Value.Contains("intname=") && !Value.Contains("int"))
                                             {
                                                 MessageBox.Show("Command " + COmmand.ButtonName + " has invalid term behind one of the / arguments", "invalid / markup");
                                                 continue;
                                             }
                                          //   Debug.WriteLine("Does this cointain int?" + Value);
                                             if (Value.Contains("intname"))
                                                 COmmand.VoiceTrigIntName.Add(ParseString(Value, "intname="));
                                             //Add the intname to a list that will be compared with the list of voice recognized ints to assign to EG: first intname will be assigned to the first int value recognized and so on
                                             continue;
                                         }
                                         //here you can add other catagories to the int modifier
                                     }
                                 }
                                 catch (Exception E)
                                 {
                                     MessageBox.Show(E.ToString() + "Command Triggering Error Is " + COmmand.ButtonName);
                                 }
                                 //return;

                             }

                             else if (S.Contains("string"))
                             {
                                 if (!S.Contains("stringname="))
                                 {
                                     MessageBox.Show("Error Loading The string Command of Button " + COmmand.ButtonName + " : The string modifier has no stringname=VALUE variable, This is required in order to pass it to the code");
                                     continue;
                                 }
                                 string[] Variables = S.Split('/');
                                 bool IsFirstToBeAdded = false;
                                 int index = -1;
                                 foreach (string ss in Variables)
                                 {
                                    // Debug.WriteLine("Split String Is :" + ss);
                                     if (ss == "string")//ignore this one, it is merely an identifier
                                     {
                                         continue;
                                     }

                                     else if (ss.Contains("stringname="))//string name Identifier used to determine wich string it will pass the result to
                                     {
                                         COmmand.VoiceTrigStringName.Add(ParseString(ss, "stringname="));//add the raw/parsed string name
                                         foreach (string SA in COmmand.VoiceTrigStringName)
                                         {
                                          //   Debug.WriteLine("stringNAME :" + SA + " ButtonName : " + COmmand.ButtonName);//remove this debug function
                                         }
                                         COmmand.VoiceTrigStringNamePosition.Add(ForeachCounter);
                                         continue;
                                     }

                                     else//It must be a Normal value
                                     {
                                         if (!IsFirstToBeAdded)
                                         {
                                             IsFirstToBeAdded = true;
                                             ChoiceArr.Add(new Choices(ss));
                                             index = ChoiceArr.Count() - 1;
                                             continue;
                                         }

                                         if (index == -1)//safety check
                                         {
                                             MessageBox.Show("something went wrong with the string name choice adding in command " + COmmand.ButtonName, "String Identifier Parser");
                                             break;
                                         }

                                         ChoiceArr[index].Add(ss);//add alternative choice
                                         continue;
                                     }
                                 }
                             }

                             else if (S.Contains('(') && S.Contains(')'))//if it contains Either keyword brackets        (song|songs)
                             {
                                 try
                                 {
                                     string Either = ParseString(S, "(", ")");   //   song|songs
                                     string[] splitEither = Either.Split('|');   //   song   songs

                                     if (splitEither[0] == "")
                                         MessageBox.Show("Invalid | markup, Did you forget to add a word on both sides of the '|' to the voicetrigger of command  " + COmmand.ButtonName, "Or statement voiceCommand String parser ");

                                     ChoiceArr.Add(new Choices(splitEither[0]));
                                     int index = ChoiceArr.Count() - 1;
                                     foreach (string STR in splitEither)
                                     {
                                         if (STR != "")
                                         {
                                             ChoiceArr[index].Add(STR);  // 'song' is added and 'songs' is an alternative
                                         }
                                         else
                                         {
                                             MessageBox.Show("Invalid | markup, Did you forget to add a word on both sides of the '|' to the voicetrigger of command  " + COmmand.ButtonName, "Or statement voiceCommand String parser ");
                                         }

                                     }
                                 }
                                 catch (Exception E)
                                 {
                                     MessageBox.Show("OOps! Something went wrong parsing your |  Command Causing it is " + COmmand.ButtonName + "  Error:  " + E.ToString(), "Or statement voiceCommand String parser ");
                                 }

                             }

                             else//if the split string does not contain int identifier it must be normal voice dialog
                             {
                                 //ChoiceArr[ChoiceArr.Count()].Add(S);
                                 if (S != "")
                                     ChoiceArr.Add(new Choices(S));
                             }

                         }
                         GrammarBuilder Tmp = new GrammarBuilder();
                         Tmp.Append("overlay");
                         foreach (Choices C in ChoiceArr)
                         {
                             Tmp.Append(C);
                         }
                         ChoiceArr = new List<Choices>();
                         DynamicGbuilder.Add(Tmp);
                       //  Debug.WriteLine(Tmp.DebugShowPhrases);
                     }

                 }

             });
            c.Add("Reload Voice Triggers");
            c.Add("Lock voice ");
            c.Add("unlock voice ");
            c.Add("Calibrate voice");
            ca.Add("Calibrate");
            ca.Add("Short sentence");
            ca.Add("grant access");
            ca.Add("deny access");
            //   cab.Add("search google for");



            GrammarBuilder gb = GrammarBuilder.Add((GrammarBuilder)"overlay", c);
            GrammarBuilder gba = new GrammarBuilder(ca);
            // GrammarBuilder Dynamic = new GrammarBuilder("overlay");
            // Dynamic.Append(cab);
            // Dynamic.AppendDictation();
            List<GrammarBuilder> Temp = new List<GrammarBuilder> { gb, gba };

            foreach (GrammarBuilder B in DynamicGbuilder)
            {
                Temp.Add(B);//add dynamic grammar builders
            }
            DynamicGbuilder.Clear();

            Choices bothChoices = new Choices(Temp.ToArray());

            //Console.WriteLine(gb.DebugShowPhrases); caused werid nullreferenceexceptions, Not needed
            //  gb.Append("overlay");
            Grammar g = new Grammar(bothChoices);



            rec = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-ZA"));
            rec.SetInputToDefaultAudioDevice();
            rec.LoadGrammar(g);
            rec.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec_SpeechRecognized);
            rec.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(Speech_detected);
            rec.RecognizeAsync(RecognizeMode.Multiple);
        }
            catch(Exception E)
            {
                MessageBox.Show("Something Went Wrong Error: " + E.ToString(), "Voice Command Loading");
            }
    }

        public static void ShutDownVoiceRec()//you COULD use rec.Dispose but here I am setting up to resstart the loading process
        {
            
            DynamicGbuilder = new List<GrammarBuilder>();
            Authentication = false;
            c = new Choices();
            ca = new Choices();
            cab =  new Choices();
            rec.Dispose();
            initializeVrec();
        }

        private static void Speech_detected(object sender, SpeechDetectedEventArgs e)
        {
         //   MainWindow.synth.Speak("speech detected");
        }
        public static string ParseString(string str,string ToReplace, string OptionalSecondary = "")//returns a string parsed out with ToReplace
        {
            if (str != null)
            {
             //   Debug.WriteLine("String To Be Parsed is " + str + " Replacing all " + ToReplace);
                string NewS = str.Replace(ToReplace, "");
                if (OptionalSecondary != "")
                {
                    NewS = str.Replace(OptionalSecondary, "");
                }
               // Debug.WriteLine(NewS);
                return NewS;
            }
            Debug.WriteLine("PARSE FAILED String was null");
            return "";
        }


        private static void rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var Timer1 = Stopwatch.StartNew();
            //  Debug.WriteLine(e.Result.Text);

            try {

                if (Authentication == true)
                {

                    if (e.Result.Confidence > ConfidenceMin)
                    {
                        if (e.Result.Text == "grant access")
                        {
                            if (player != null)
                            {
                                player.Play();
                                MainWindow.synth.SpeakAsyncCancelAll();
                            }
                            Ccompiler.Compile(CommandBuffer);
                            Authentication = false;
                        }
                        else if (e.Result.Text == "deny access")
                        {
                            Authentication = false;
                            MainWindow.Speak("ok");
                            return;
                        }
                    }
                }
                //If authentication is needed all other commands but yes/no are disabled untill confirmation
                else
                {

                    if (lockOverlay == false)
                    {


                        if (e.Result.Text == "overlay Calibrate voice")
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
                                MainWindow.Speak("Repeat after me. Calibrate");
                                safety = true;
                                Debug.WriteLine("returning 0");

                                return;

                            }
                            if (e.Result.Text == "Calibrate" && configureP2 == false)
                            {
                                ConfidenceHolder = e.Result.Confidence;
                                configureP2 = true;
                                MainWindow.Speak("good, now say , short sentence");
                                Debug.WriteLine("returning 1");
                                return;
                            }
                            else if (e.Result.Text == "Short sentence" && configureP2 == true)
                            {
                                ConfidenceHolder = e.Result.Confidence;
                                configureP2 = false;
                                Configure = false;
                                safety = false;
                                MainWindow.Speak("Got it, Calibrating");
                                float Confidence2 = e.Result.Confidence;

                                ConfidenceMin = ((ConfidenceHolder + Confidence2) / 2) - 0.1f;
                                Console.WriteLine("confidence Minimum is now " + ConfidenceMin);
                                Debug.WriteLine("returning 2");

                                return;
                            }
                        }
                    }
                }
                Console.WriteLine("confidence is :" + e.Result.Confidence);
                if (e.Result.Confidence > ConfidenceMin)
                {

                    MainWindow.synth.SpeakAsyncCancelAll();//cancel speech


                    if (e.Result.Text == "overlay Lock voice")
                    {
                        MainWindow.Speak("Locked");
                        lockOverlay = true;
                        return;
                    }
                    else if (e.Result.Text == "overlay unlock voice")
                    {
                        if (lockOverlay == true)
                        {
                            MainWindow.Speak("Unlocked");
                            lockOverlay = false;
                            return;

                        }
                        else
                        {
                            MainWindow.Speak("Already unlocked");
                            return;

                        }
                    }


                    if (lockOverlay == true && e.Result.Text.Contains("overlay"))
                    {
                        if (!Shutup)
                            MainWindow.Speak("VR Locked");
                        return;
                    }

                    if (lockOverlay == false)
                    {
                        Console.WriteLine(e.Result.Text);


                        CancellationTokenSource cts = new CancellationTokenSource();
                        ParallelOptions po = new ParallelOptions();
                        po.MaxDegreeOfParallelism = 20; // max threads
                        po.CancellationToken = cts.Token;


                        Parallel.ForEach(MainWindow.Command, po, (CO) => //(Command CO in MainWindow.Command)
                        {
                            if (CO.VoiceTrigger != null)
                            {

                                if (!CO.VoiceTrigger.Contains("&") && !CO.VoiceTrigger.Contains("|"))
                                {
                                    string CommandAnswer = "overlay " + ParseString(CO.VoiceTrigger, " ").ToLower();
                                //  Debug.WriteLine("CommandAnswer is " + CommandAnswer + "  :  voiceTxt is : " + e.Result.Text);
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
                                            po.CancellationToken.ThrowIfCancellationRequested(); //Cancel Because Correct Command Found
                                    }

                                        else
                                        {
                                            Authentication = true;
                                            CommandBuffer = CO.CommandContent;
                                            MainWindow.Speak(CO.ButtonName + "needs access");

                                        }
                                        po.CancellationToken.ThrowIfCancellationRequested(); // also Cancel The loop Because autherization is needed
                                }
                                    return;
                                }
                                if (IsCorrectCommand(e.Result.Text, CO.VoiceTrigger, CO.ButtonName))
                                {
                                    FinalCleanup(e.Result.Text, CO.CommandContent, CO, CO.VoiceTrigIntName, CO.VoiceTrigStringName);//REMEMBER TO ADD DETECTION TO TEST WHETHER IT IS THE CORRECT COMMAND
                                po.CancellationToken.ThrowIfCancellationRequested(); // Cancel Because Correct Command Found
                            }
                            }
                            Debug.WriteLine("One of the commands in the observable list being checked has no voice trigger");
                            return;
                        });


                    }
                }
            }
            finally
            {
                Timer1.Stop();
                Debug.WriteLine("Recognition time is " + Timer1.ElapsedMilliseconds);
            }
            }

        private static void FinalCleanup(string VoiceRECtxt, string CommandToCompile, Command CurrentCommand, List<string> IntNames = null, List<string> StringNames = null)//clean up, get and set value to pass into the compiler
        {
            var timer3 = Stopwatch.StartNew();
            try {
                try {
                    List<string> Values = new List<string>();
                    List<string> StringValues = new List<string>();//String values

                    if (IntNames != null)
                    {

                        var match = Regex.Matches(VoiceRECtxt, @"-?\d+");

                        foreach (var _match in match)
                        {
                            Values.Add(_match.ToString());

                        }
                        // Ccompiler.Compile(CommandToCompile, IntNames, Values);
                    }

                    if (StringNames != null)
                    {
                        string[] SplitVREC = VoiceRECtxt.Split(' ');
                        //  Debug.WriteLine("VoiceRecTxt :" + VoiceRECtxt);
                        int index = 0;
                        foreach (string S in StringNames)
                        {
                            if (!S.Contains("overlay"))
                            {
                                //    Debug.WriteLine("string Names :" + S);

                                StringValues.Add(SplitVREC[CurrentCommand.VoiceTrigStringNamePosition[index] + 1]);
                                //   Debug.WriteLine("StringValue Added : " + SplitVREC[CurrentCommand.VoiceTrigStringNamePosition[index] + 1]);
                                index += 1;
                            }
                        }
                    }


                    if (Values != null && StringValues != null)
                    {
                        Ccompiler.Compile(CommandToCompile, IntNames, Values, StringNames, StringValues);//NEED TO HANDLE THESE STRING VALUES
                    }
                    else if (Values != null)
                    {
                        Ccompiler.Compile(CommandToCompile, IntNames, Values);
                    }
                    else if (StringNames != null)
                    {
                        Ccompiler.Compile(CommandToCompile, null, null, StringNames, StringValues);
                    }

                    Debug.WriteLine("All Values were Null");

                }
                catch (Exception E)
                {
                    MessageBox.Show(E.ToString(), "Final Parse Before Compilation");
                }
            }
            finally
            {
                timer3.Stop();
                Debug.WriteLine("FinalCleanup Took " + timer3.ElapsedMilliseconds);
            }
        }

        private static bool IsCorrectCommand(String VoiceRecTxt, string VoiceTrigger, string CommandName = "")
        {
            var Timer2 = Stopwatch.StartNew();
            try {
                
            try
            {

                string voce = ParseString(VoiceRecTxt, " ");
                string Trig = ParseString(VoiceTrigger, " ");

                List<string> VoceTrig = new List<string>(Trig.ToLower().Split('&')).Where(x => !x.Contains("intname") && !x.Contains("overlay")).ToList(); ;// test   int/min=0/max=5/intname=banana <== this is not added   (banana|bananas)
                int NumberCorrect = 0;
                foreach (string VOCE in VoceTrig)
                {
                    // Debug.WriteLine("VOCE is " + VOCE);
                    if (VOCE == "")
                    {
                        continue;
                    }

                    if (VOCE.Contains('|'))
                    {
                        string Parsed = ParseString(VOCE, "(", ")");
                        string[] SplitS = Parsed.Split('|');

                        foreach (string S in SplitS)
                        {
                            if (VoiceRecTxt.Contains(S))//This is for or statements EG: voice rec is : overlay test 1 banana   and or statement is (banana|bananas) Then banana will bee seen as a match
                                NumberCorrect += 1;
                        }
                        continue;
                    }
                    else if (VOCE.Contains('/') && VOCE.Contains("stringname"))//This string variable Cycles through the options and adds to numberCorrect if it matches
                    {
                        string[] SplitS = VOCE.Split('/');

                        foreach (string S in SplitS)
                        {
                            if (!S.Contains("stringname") && S != "string")
                            {
                                if (VoiceRecTxt.Contains(S))
                                {
                                    NumberCorrect += 1;
                                    break;
                                }
                            }
                        }
                        continue;
                    }


                    if (VOCE != "overlay" && VoiceRecTxt.Contains(VOCE))
                    {
                        //     Debug.WriteLine("VALUE IS TRUE " + CommandName + " VoiceDiag : " + VoiceRecTxt + "  VOCE : " + VOCE);
                        NumberCorrect += 1;
                        //  Debug.WriteLine(NumberCorrect);
                    }



                }
                //   Debug.WriteLine("NumberCorrect is " + NumberCorrect + " Size of array is " + VoceTrig.Count());
                if (NumberCorrect == VoceTrig.Count())//if all except dynamic arguments are met, it is assumed true
                {
                    Debug.WriteLine("Identification correct for command,  " + VoiceRecTxt);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception E)
            {
                MessageBox.Show("Command With paramaters Caused The following error on evaluation: " + E.ToString());
            }
            //Debug.WriteLine("VALUE IS FALSE " + CommandName);
            return false;
        }
            finally
            {
                Timer2.Stop();
                Debug.WriteLine("IscorrectCommand Took " + Timer2.ElapsedMilliseconds + " ms");
            }
        }


    }

    }


