
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

using Renci.SshNet;
using dotNET_RSA_KEY;

namespace AUTODECK
{

    class AUTODECK_MAIN
    {
        //RUNDECK PROC VARS
        static Process RunDeckProcess = new Process();
        //public static Boolean DONE = false;
        public static Boolean GO = false;
        public static Boolean MAKE = false;
        public static Boolean COMPLETE = false;

        public static Boolean bit64 = false;

        public static Boolean PGO = false;
        public static Boolean pbit64 = false;
        public static Boolean GotPutty = false;

        public static Boolean SHOW_PUTTY = false;
        public static Boolean SHOW_RD = false;

        //DIR VARS
        public static string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string dir0 = desktop + @"\AutoDECK";
        public static string dir1 = desktop + @"\AutoDECK\SSH_KEY";
        public static string dir2 = desktop + @"\AutoDECK\RUNDECK";
        public static string dir3 = desktop + @"\AutoDECK\BIN";

        //FILES
        public static string bhFile = dir0 + @"\FAILED_hostfile.txt";

        //[[#AutoDECK]] SSH KEY
        public static StringBuilder sshkey = new StringBuilder();
        public static string prvkeyFile = null;
        public static string pubkeyFile = null;
        public static object PrvKey = null;

        //[[#AutoDECK]] PASSWORDS
        public static List<object> pass = new List<object>(); //PASSWORD LIST

        //[[#AutoDECK]] COUNT ALIVE THREADS PAUSE LOOP UNTIL THAN MAX
        public static int OD = 0;
        public static int OVER = 200; //OVER DRIVE
        public static int TMAX = 20; //THREAD MAX
        public static int INJECT = 10; //THREAD MAX
        public static int TRUN = 0; //RUNNING THREAD COUNT

        //[[#AutoDECK]] THREAD POOL LOCK
        public static Object thisLock = new Object();
        public static Object wordLock = new Object();
        public static Object listLock = new Object();
        public static Object Lock_MakeXML = new Object();


        static void Main(string[] args)
        {
            int screenWidth = 120;
            int screenHeight = screenWidth / 4;
            Console.Title = "AutoDECK";
            Console.WindowHeight = Console.LargestWindowHeight > screenHeight ? screenHeight : Console.LargestWindowHeight / 2;
            Console.WindowWidth = Console.LargestWindowWidth > screenWidth ? screenWidth : Console.LargestWindowWidth - 10;
            Console.SetWindowSize(Console.WindowWidth, Console.WindowHeight);
            Console.BufferWidth = Console.WindowWidth;
            //Console.BufferHeight = Console.WindowHeight;
            Console.BufferHeight = Int16.MaxValue - 1; //MAX OUT CONSOLE BUFFER
            Console.Clear();
            Console.ResetColor();

            //PROC INFO
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "AutoDECK_MainThread"; // to avoid a possible InvalidOperationException. 
            }
            else
            {
                Console.WriteLine("Unable to name a previously " + "named thread.");
            }
            Console.WriteLine("PROCESS NAME: " + Process.GetCurrentProcess()); //LIST EXE NAME
            Console.WriteLine("[Thread.CurrentThread.Name]: " + Thread.CurrentThread.Name); //LIST THREAD NAME


            //[[#AutoDECK]] MAKE DIRS
            if (!Directory.Exists(dir0))
            {
                DirectoryInfo InDir0 = Directory.CreateDirectory(dir0);
                DirectoryInfo InDir1 = Directory.CreateDirectory(dir1);
                DirectoryInfo InDir2 = Directory.CreateDirectory(dir2);
                DirectoryInfo InDir3 = Directory.CreateDirectory(dir3);
                Console.WriteLine("MADE DIR: " + dir0);
                Console.WriteLine("MADE DIR: " + dir1);
                Console.WriteLine("MADE DIR: " + dir2);
                Console.WriteLine("MADE DIR: " + dir3);
            }

            //WEBCLIENT (DOWNLOAD RUNDECK)
            Thread webthread = new Thread(() => WebClient());
            webthread.Name = "GET_RUNDECK_WEB_THREAD";
            webthread.Start();

            //WEBCLIENT (DOWNLOAD PUTTY)
            Thread getpthread = new Thread(() => GetPutty());
            getpthread.Name = "GET_PUTTY_WEB_THREAD";
            getpthread.Start();

            //MAKE SSH KEYS : THREAD
            string ssh = null; 
            Thread makekeythread = new Thread(() => ssh = MAKE_RSA_KEY.MAKE_KEY());
            makekeythread.Name = "MAKE_RSA_KEY_THREAD";
            makekeythread.Start();
            //makekeythread.Join(); //RETURN SSH KEY LATER
            //sshkey.Append(ssh);

            //WAIT HERE FOR JAVA INSTALL
            GotJAVA(); //MAIN THREAD STOP

            //[[#AutoDECK]] MAKE CALLER BAT FILES
            Directory.SetCurrentDirectory(dir0);
            Console.WriteLine("[BAT FILE] Current directory: {0}", Directory.GetCurrentDirectory());
            //string rdb = dir0 + @"RUNDECK-START.bat";
            Console.WriteLine("MAKING RUNDECK BAT FILE");
            StringBuilder rdeck = new StringBuilder();
            try
            {
                System.IO.StreamWriter rdfile = new System.IO.StreamWriter(@"RUNDECK-START.bat");
                rdfile.WriteLine("cd " + dir2);
                rdeck.Append('"');
                if (bit64)
                {
                    rdeck.Append(@"C:\Program Files (x86)\Java\jre6\bin\java.exe");
                }
                else
                {
                    rdeck.Append(@"C:\Program Files\Java\jre6\bin\java.exe");
                }
                rdeck.Append('"');
                rdeck.Append(" -jar rundeck-launcher-2.3.2.jar");
                rdfile.WriteLine(rdeck);
                rdfile.WriteLine("pause");
                rdfile.Close();
                Console.WriteLine(".........DONE MAKING RUNDECK BAT FILE");
            }catch(Exception bat)
            {
                Console.WriteLine("PROBLEM WITH RUNDECK BAT FILE:\n {0}",bat);
            }
            
     
            //[[#AutoDECK]] START UP NOTEPAD FOR HOST AND PASSWORD LIST
            string hFile = dir0 + @"\hostfile.txt";
            string pFile = dir0 + @"\passfile.txt";

            if (!File.Exists(hFile) || !File.Exists(pFile))
            {
                System.IO.StreamWriter xpfile = new System.IO.StreamWriter(pFile);
                xpfile.WriteLine("REMOVE THESE LINES: [ PUT PASSWORD LIST HERE ]");
                xpfile.WriteLine("password1");
                xpfile.WriteLine("password2");
                xpfile.WriteLine("password3");
                xpfile.WriteLine("root");
                xpfile.Write("123456");
                xpfile.Close();

                System.IO.StreamWriter xhfile = new System.IO.StreamWriter(hFile);
                xhfile.WriteLine("REMOVE THESE LINES: [ PUT HOST LIST HERE ]");
                xhfile.WriteLine("hostname1");
                xhfile.WriteLine("hostname2");
                xhfile.WriteLine("hostname3");
                xhfile.WriteLine("use-hostname-or-ipaddress");
                xhfile.WriteLine("14.0.0.126");
                xhfile.WriteLine("14.0.0.138");
                xhfile.WriteLine("14.0.0.131");
                xhfile.WriteLine("14.0.0.103");
                xhfile.WriteLine("14.0.0.141");
                xhfile.Write("14.0.0.130");
                xhfile.Close();

                System.Diagnostics.Process HostLlist = new System.Diagnostics.Process();
                HostLlist.StartInfo.FileName = "notepad";
                HostLlist.StartInfo.Arguments = hFile;

                Thread notethread = new Thread(() => HostLlist.Start());
                notethread.Name = "NOTEPAD_THREAD";
                notethread.Start();

                System.Diagnostics.Process PassList = new System.Diagnostics.Process();
                PassList.StartInfo.FileName = "notepad";
                PassList.StartInfo.Arguments = pFile;
                PassList.Start();

                PassList.WaitForExit();
                HostLlist.WaitForExit();
            }

            
            //[[#AutoDECK]] LOAD LISTS
            Console.WriteLine("[GET HOST INFO]");
            string HOSTFILE = dir0 + @"\hostfile.txt";
            var hostlist = File.ReadAllLines(HOSTFILE);
            List<object> hosts = new List<object>(hostlist); //HOST LIST
            //hosts.Add("14.0.0.126");

            Console.WriteLine("[GET PASSWORD INFO]");
            string PASSFILE = dir0 + @"\passfile.txt";
            var passlist = File.ReadAllLines(PASSFILE);
            foreach (var s in passlist) pass.Add(s); //PASSWD LIST 
            //pass.Add("passwd");

            List<object> user = new List<object>(); //USER LIST [NOT USED FOR NOW]
            user.Add("root");


            //WAIT HERE FOR SSH KEY
            makekeythread.Join(); //RETURN SSHKEY HERE
            sshkey.Append(ssh);


            //RUN SSH CLIENT CONF THREADS
            //These Threads also MAKE/UPDATE Rundeck resource.xml
            //[[#AutoDECK]] (My) THREAD POOL : RUN NO MORE THAN WE NEED TO
            int t = 0;
            foreach (string host in hosts) //MAKE THREAD COUNT
                              t++;
            Thread[] threads = new Thread[t]; //THREAD ARRAY
            Alert.White("QUEUED UP [" + t + "] THREADS.."); //SHOW HOW MANY THREADS

            int i = 0; //THREAD COUNT
            foreach (string host in hosts)
            {
                while (TRUN >= TMAX) 
                {
                    Thread.Sleep(1);
                    OD++;
                    if (OD == 1000)
                    {
                        if (TMAX <= OVER) 
                        {
                            TMAX += INJECT; 
                            OD = 0; 
                            Alert.White("ADDED MORE THREADS#: " + TMAX); //ADDING THREADS
                        }
                    }
                        
                } //BLOCK THREADS AT MAX
                Alert.Green("[MAX RUNNING THREADS@: "+TMAX.ToString()+"] RUNNING THREAD #: " + i);
                threads[i] = new Thread(() => tssh(host));
                threads[i].Name = i.ToString(); //SET THREAD NUM [KEEP THIS!!!]
                threads[i].Start();
                Alert.PRO(i, t); //PROGRESS ALERTS
                i++;
            }


            //MAKE RUNDECK CONFIG FILES
            Thread cfgthread = new Thread(() => MakeCFG());
            cfgthread.Name = "RUNDECK_CFG_THREAD";
            cfgthread.Start();


            if ((!GO) && (!COMPLETE))
            {
                Alert.DarkMagenta("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n[[WAITING FOR RUNDECK DOWNLOAD]]");
                SHOW_RD = true;
            }

            //[[#AutoDECK]] START UP RUNDECK PROC w/FORK
            while (!GO) { }
            while (!COMPLETE) { }
            Directory.SetCurrentDirectory(dir2);
            Console.WriteLine("[START RUNDECK] Current directory: {0}", Directory.GetCurrentDirectory());
            System.Diagnostics.Process _RunDeck = new System.Diagnostics.Process();
            _RunDeck.StartInfo.FileName = "cmd";
            _RunDeck.StartInfo.Arguments = "/c START cmd /T:6b /k " + rdeck;
            Thread rundeckthread = new Thread(() => _RunDeck.Start());
            rundeckthread.Name = "RUNDECK THREAD";
            rundeckthread.Start();

            //CONNECT TO RUNDECK URL
            Thread urlthread = new Thread(() => Rundeck_URL());
            urlthread.Name = "URL_THREAD";
            urlthread.Start();

            //CHECK PROGRESS OF SSH CLIENT CONF THREADS
            int tha = t - 1;
            int thd = t - 1;
            for (; tha > 0; tha--)
            {
                //Console.WriteLine("THREAD DEAD: " + tha);
                if (!threads[tha].IsAlive)
                {
                    thd--;
                }
                if (threads[tha].IsAlive)
                    Alert.DarkMagenta("[[ WAITING ON SSH CLIENT CONF THREAD ]]: " + threads[tha].Name.ToString());
            }

            //WAIT FOR SSH CLIENT CONF THREADS
            int TH = 0; //THREAD COUNT
            foreach (string host in hosts)
            {
                threads[TH].Join();
                TH++;
            }

            //#?
            //END           
            Console.ResetColor();
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nALL DONE\nGOTO: http://127.0.0.1:4440\nUserName: admin\nPassWord: admin");
            Console.WriteLine("[[SEE: http://rundeck.org/docs/]]");
            Console.WriteLine("\n\n[[ (Optional) INSTALL FROM DIR: {0} ]]\n[Press p] (Optional) to Install Putty \n[Press q] to QUIT!\n\n", dir3);
            Alert.DarkMagenta("\n[[ WAITING FOR RUNDECK SERVER ]]");
            if (File.Exists(bhFile))
                Alert.Red("[[ FAILED HOST LIST ]]: " + bhFile.ToString());
     
            //SHOW_RD = true;

            do
            {
                ConsoleKeyInfo cki;
                cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.P)
                {
                    installputty();
                    SHOW_PUTTY = true;
                    break;
                }
                if (cki.Key == ConsoleKey.Q)
                    break;

                Console.WriteLine("[Press p] (Optional) to Install Putty\n[Press q] to QUIT!");
            }
            while (true);
            

        }//MAIN END


        //[[#AutoDECK]]
        public class goodword //GOOD PASSWORD LIST
        {
            public object ghost { get; set; } 
            public object guser { get; set; } 
            public object gpassword { get; set; } 
        }
        //new goodword {host = "", user = "", password="" }
        public static List<goodword> myword = new List<goodword> { }; //MAKE LIST FROM CLASS OBJ

        public static void addPASS(object host, object user, object pw)
        {
            lock (wordLock)
            {
                myword.Add(new goodword { ghost = host, guser = user, gpassword = pw }); //UPDATE GOOD PASSWORD LIST
            }
        }

        //[[#AutoDECK]] THREAD POOL
        public static void IsRUN() //ADD TO THREAD RUNNING COUNT
        {
            lock (thisLock)
            {
                TRUN++;
            }
        }

        //[[#AutoDECK]] THREAD POOL
        public static void IsNotRUN() //SUBTRACT FROM THREAD RUNNING COUNT
        {
            lock (thisLock)
            {
                TRUN--;
                if(TMAX >= 60)
                    TMAX -= 5;
                else if (TMAX >= 51)
                    TMAX -= 1;
            }
        }

        /*
         *Shuffle the starting password to the same index# as the host#
         *in case a known (host == password) list is added
         *Otherwise it iterates the whole list 
        */ 
        public static List<object> shuffle() 
        {
            lock (listLock)
            {
                List<object> mypass = new List<object>();
                mypass = pass;
                List<object> newmypass = new List<object>();
                newmypass = pass;
                List<object> usemypass = new List<object>();

                int cp = 0;
                foreach (string p in mypass) //LIST
                {
                    int idx = cp;
                    cp++;
                    int tc = Convert.ToInt16(Thread.CurrentThread.Name); //USE THREAD NAME AS INDEX
                    if (idx == tc)
                    {
                        newmypass.RemoveAt(idx); //SHUFFLE: Remove Item Here
                        newmypass.Insert(0,p); //ADD Item to top of list
                        break;
                    }
                }
                usemypass = newmypass; //LAST COPY
                /*
                //TEST NEW LIST
                int osize = newmypass.Count;
                int isize = mypass.Count;
                foreach (string i in usemypass)
                {
                    Console.WriteLine("{1} NEW LIST: {0}", i, Thread.CurrentThread.Name); //DEBUG VIEW
                    break;
                }
                */
                List<object> sh = usemypass; //REORDERED LIST
                return sh;
            }
        }

        //SAVE BAD HOSTS 
        public static Object LOCK_BADHOST = new Object();
        public static void BADHOST(string host)
        {
            lock (LOCK_BADHOST)
            {
                //string bhFile = dir0 + @"\FAILED_hostfile.txt";
                System.IO.StreamWriter badfile = new System.IO.StreamWriter(bhFile,true);
                badfile.WriteLine(host);
                badfile.Close();
                Console.WriteLine("THREAD# {0} [ FAILED HOST ({1}) SAVED TO LIST: {2} ]", Thread.CurrentThread.Name, host, bhFile);
            }
        }

        //NEED TO MAKE A CONSOLE MANAGER THREAD FOR ALL THE COLORS TO WORK
        public static Object Lock_Alert = new Object();
        public static class Alert
        {
            public static void MSG(StringBuilder msg, object color)
            {
                lock(Lock_Alert)//Need lock to make it thread safe
                {
                    Console.ForegroundColor = (ConsoleColor)color;
                    Console.WriteLine(msg.ToString());
                    Console.ResetColor(); 
                }

            }

            public static void MSG(StringBuilder msg, object color, string ok)
            {
                lock (Lock_Alert)//Need lock to make it thread safe
                {
                    Console.ForegroundColor = (ConsoleColor)color;
                    Console.Write(msg.ToString());
                    Console.ResetColor();
                }

            }

            public static void SPIN()
            {
                int counter = 0;
                Console.Write("[ Working.... "); //ADDED THIS SO NEED "-14"
                counter++;
                switch (counter % 4)
                {
                    case 0: Console.Write("/"); counter = 0; break;
                    case 1: Console.Write("-"); break;
                    case 2: Console.Write("\\"); break;
                    case 3: Console.Write("|"); break;
                }
                Console.Write(" ]"); //ADDED THIS SO NEED "-3" 
            }

            public static void PRO(int progress, int total) //#?
            {
                StringBuilder alert = new StringBuilder();
                alert.AppendFormat("[ THREAD PROGRESS: " + progress.ToString() + " of " + total.ToString() + " ] ");
                object color = ConsoleColor.White;
                MSG(alert, color, "OK"); //blanks at the end remove any excess
                alert.Clear();
            }

            public static void WEB(params string[] args)//#?
            {
                StringBuilder alert = new StringBuilder();
                for (int i = 0; i < args.Length; i++)
                {
                    alert.AppendFormat(args[i].ToString());
                }
                object color = ConsoleColor.White;
                MSG(alert, color, "OK");
                alert.Clear();
            }

                public static void Red(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.Red;
                    MSG(alert,color);
                    alert.Clear();
                }

                public static void Yellow(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.Yellow;
                    MSG(alert, color);
                    alert.Clear();
                }

                public static void Blue(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.Blue;
                    MSG(alert, color);
                    alert.Clear();
                }

                public static void Green(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.Green;
                    MSG(alert, color);
                    alert.Clear();
                }

                public static void Cyan(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.Cyan;
                    MSG(alert, color);
                    alert.Clear();
                }

                public static void DarkMagenta(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.DarkMagenta;
                    MSG(alert, color);
                    alert.Clear();
                }

                public static void Gray(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.DarkGray;
                    MSG(alert, color);
                    alert.Clear();
                }

                public static void White(params string[] args)
                {
                    StringBuilder alert = new StringBuilder();
                    for (int i = 0; i < args.Length; i++)
                    {
                        alert.AppendFormat(args[i].ToString());
                    }
                    object color = ConsoleColor.White;
                    MSG(alert, color);
                    alert.Clear();
                }
        
        }

        //[[#AutoDECK]] SSH THREADS
        public static void tssh(string host)
        {
            IsRUN(); //UPDATE THREAD POOL COUNTER

            List<object> thepass = new List<object>();
            thepass = shuffle(); //DO THE SHUFFLE

            Boolean success = false;
            try
            {
                string pinghost = host; // PING HOST
                Ping pingreq = new Ping();
                PingReply rep = pingreq.Send(pinghost);
                //Console.WriteLine("THREAD# {0} Pinging {1} [{2}]", Thread.CurrentThread.Name, pinghost, rep.Address.ToString());
                //Console.WriteLine("THREAD# {0} Reply From {1} : time={2} TTL={3}",Thread.CurrentThread.Name, rep.Address.ToString(), rep.RoundtripTime, rep.Options.Ttl);
                //Console.WriteLine(rep.Status);
                if ((rep.Status.ToString() != "DestinationHostUnreachable") && (rep.Status.ToString() != "TimedOut"))
                    success = true;
                else
                {
                    Alert.Red("THREAD# " + Thread.CurrentThread.Name.ToString() + " [THREAD STOPED] => PING FAILED (HostUnreachable): " + host.ToString());
                    BADHOST(host);
                }
            }
            catch
            {
                Alert.Red("THREAD# " + Thread.CurrentThread.Name.ToString() + " PING FAILED: " + host.ToString());
            }
            finally
            {
                if (success)
                {
                    try
                    {
                        Alert.White("THREAD# " + Thread.CurrentThread.Name.ToString() + " START: SSH TO HOST: " + host.ToString());
                        //Console.WriteLine(">> {0} START: SSH TO HOST: {1} <<", Thread.CurrentThread.Name, host);
                        foreach (string pw in thepass) //LOOP OVER PASSWORD LIST
                        {
                            try
                            {
                                string user = "root"; //SET UID
                                using (var client = new SshClient(host, user, pw))
                                {
                                    try
                                    {
                                        var ssc = new SshClient(host, user, pw);
                                        client.Connect(); //TRY CONNECTION
                                        //Console.WriteLine("{0} CONNECTED: [GOOD PASSWORD: {2}] [HOST: {1}]", Thread.CurrentThread.Name, host, pw);

                                        List<goodword> aword = new List<goodword> { };
                                        aword.Add(new goodword { ghost = host, guser = user, gpassword = pw }); //LIST FOR "LATER" UPDATING CONFIGS
                                        MakeXML(aword);
                                        //Console.WriteLine("[DONE]: {0}", Thread.CurrentThread.Name);
                                    }
                                    catch
                                    {
                                        Alert.Yellow("THREAD# " + Thread.CurrentThread.Name.ToString() + " SSH CONNECTION FAILURE (BAD PASSWORD?): " + host.ToString());
                                        if (pw.Equals(thepass[thepass.Count - 1])) //foreach (string pw in thepass)
                                        {
                                            BADHOST(host);
                                            Alert.Red("THREAD# " + Thread.CurrentThread.Name.ToString() + " ALL PASSWORDS FAILED FOR HOST: " + host.ToString());
                                        }
                                        continue; //IF CONNECTION FAILS DO NEXT LOOP
                                    }

                                    StringBuilder sshcmd = new StringBuilder(); //BUILD CLI STRINGS
                                    sshcmd.Append(" uname -a;");
                                    sshcmd.Append(" mkdir -p ~/.ssh;");
                                    sshcmd.Append(" chmod 700 .ssh;");
                                    sshcmd.Append(" touch ~/.ssh/authorized_keys;");
                                    sshcmd.Append(" touch ~/.ssh/authorized_keys2;");
                                    sshcmd.Append(" chmod 600 ~/.ssh/authorized_keys;");
                                    sshcmd.Append(" chmod 600 ~/.ssh/authorized_key2;");
                                    sshcmd.Append(" echo '" + sshkey + "' >> ~/.ssh/authorized_keys;");
                                    sshcmd.Append(" echo '" + sshkey + "' >> ~/.ssh/authorized_keys2;");
                                    sshcmd.Append(" cat ~/.ssh/authorized_keys;");
                                    sshcmd.Append(" cat ~/.ssh/authorized_keys2;");

                                    var terminal = client.RunCommand(sshcmd.ToString());

                                    var output = terminal.Result;
                                    //Console.WriteLine(output);
                                    client.Disconnect();
                                    client.Dispose();
                                    break; //IF IT WORKS BREAK LOOP
                                }
                            }
                            catch
                            {
                                Alert.Red("THREAD# " + Thread.CurrentThread.Name.ToString() + " ALL PASSWORDS FAILED FOR HOST: " + host.ToString());
                            }
                            finally
                            {
                                //NONE
                            }
                        }
                    }
                    catch
                    {
                        //NONE
                    }
                    finally
                    {
                        //NONE

                        
                    }
                    //Alert.Gray("THREAD# " + Thread.CurrentThread.Name.ToString() + " STOPED @HOST: " + host.ToString());
                }
            }
            IsNotRUN(); //UPDATE THREAD POOL COUNTER
        }


        public static void MakeCFG()
        {
            //while (TRUN != 0){}; //WAIT FOR THREAD POOL
            while (!GO){}; //WAIT FOR GO

            string autoETC = dir2 + @"\projects\AutoDECK\etc\";
            Directory.CreateDirectory(autoETC);
            Directory.SetCurrentDirectory(autoETC);
            //Console.WriteLine("Current directory: {0}", Directory.GetCurrentDirectory());

            //GEN STRINGS 
            string pattern = @"\\";
            string replacement = @"\\";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(dir2 + @"\projects\AutoDECK\etc\resources.xml", replacement);
            string rdeck = result.Replace(":", @"\:");
            //Console.WriteLine(rdeck);

            string npattern = @"\\";
            string nreplacement = @"\\";
            Regex nrgx = new Regex(npattern);
            string nresult = nrgx.Replace(dir1 + @"\AutoDECK_dsa.ppk", nreplacement);
            string rdkey = nresult.Replace(":", @"\:");
            //Console.WriteLine(rdkey);

            //[[#AutoDECK]] WRITE RUNDECK project.properties
            //Console.WriteLine("MAKING project.properties FILE");
            Alert.White("MAKING project.properties FILE");
            System.IO.StreamWriter xfile = new System.IO.StreamWriter(@"project.properties");
            StringBuilder xb = new StringBuilder();
            xb.Append("#Project AutoDECK configuration, generated by AutoDECK \r\n");
            xb.Append("project.name=AutoDECK\r\n");
            xb.Append("resources.source.1.config.requireFileExists=false\r\n");
            xb.Append("project.ssh-authentication=privateKey\r\n");
            xb.Append("service.NodeExecutor.default.provider=jsch-ssh\r\n");
            xb.Append("resources.source.1.config.includeServerNode=false\r\n");
            xb.Append("resources.source.1.config.generateFileAutomatically=true\r\n");
            xb.Append("resources.source.1.config.format=resourcexml\r\n");
            xb.Append("resources.source.1.config.file=");
            xb.Append(rdeck);
            xb.Append("\r\n");
            xb.Append("project.ssh-keypath=");
            xb.Append(rdkey);
            xb.Append("\r\n");
            xb.Append("service.FileCopier.default.provider=jsch-scp\r\n");
            xb.Append("resources.source.1.type=file\r\n");
            xfile.WriteLine(xb);
            xfile.Close();
            Console.WriteLine(".........DONE MAKING project.properties FILE");

            COMPLETE = true;
        }

        public static void MakeXML(List<goodword> aword)
        {
            lock (Lock_MakeXML)//NO WAIT JUST LOCK
            {
                Alert.Cyan("[MAKING/UPDATING RUNDECK XML FILE] I AM THREAD:" + Thread.CurrentThread.Name.ToString());

                foreach (goodword w in aword) {
                    addPASS(w.ghost, w.guser, w.gpassword);
                }

                string autoETC = dir2 + @"\projects\AutoDECK\etc\";
                Directory.CreateDirectory(autoETC);

                XmlTextWriter xwr;
                string xfilename = autoETC + "resources.xml";

                if (File.Exists(xfilename))
                {
                    XDocument xDocument = XDocument.Load(xfilename);
                    XElement root = xDocument.Element("project");
                    IEnumerable<XElement> rows = root.Descendants("node");
                    XElement firstRow = rows.First(); //START AT TOP (FASTER)
                    //XElement lastRow = rows.Last(); //START AT BOTTOM

                    foreach (goodword w in aword) //foreach (goodword w in myword)
                    {
                        firstRow.AddBeforeSelf(
                            //lastRow.AddAfterSelf(
                           new XElement("node",
                             new XAttribute("name", (string)w.ghost),
                             new XAttribute("description", ""),
                             new XAttribute("tags", ""),
                             new XAttribute("hostname", (string)w.ghost),
                             new XAttribute("osArch", ""),
                             new XAttribute("osFamily", ""),
                             new XAttribute("osName", ""),
                             new XAttribute("osVersion", ""),
                             new XAttribute("username", (string)w.guser)));
                    }
                    xDocument.Save(xfilename);
                    myword.Clear();
                }
                else
                {
                    xwr = new XmlTextWriter(xfilename, Encoding.UTF8);
                    xwr.Formatting = Formatting.Indented;
                    xwr.WriteStartDocument();
                    xwr.WriteStartElement("project"); //<project>

                    foreach (goodword w in aword)
                    {
                        xwr.WriteStartElement("node"); //<name>
                        xwr.WriteAttributeString("name", (string)w.ghost); //Attribute Must be before
                        xwr.WriteAttributeString("description", "");
                        xwr.WriteAttributeString("tags", "");
                        xwr.WriteAttributeString("hostname", (string)w.ghost); //Attribute Must be after Element
                        xwr.WriteAttributeString("osArch", "");
                        xwr.WriteAttributeString("osFamily", "");
                        xwr.WriteAttributeString("osName", "");
                        xwr.WriteAttributeString("osVersion", "");
                        xwr.WriteAttributeString("username", (string)w.guser); //Attribute Must be after Element
                        xwr.WriteEndElement();
                    }
                    xwr.Close();
                    myword.Clear();
                }
                Alert.White(".....DONE MAKING/UPDATING RUNDECK XML FILE");
                //COMPLETE = true;
            }
        }

        public static void GotJAVA()
        {
            //[[#AutoDECK]] GOT JAVA
            Boolean GotJava = false;
            string java64 = @"C:\Program Files (x86)\Java\jre6\bin";
            string java86 = @"C:\Program Files\Java\jre6\bin";

            List<object> javaURL = new List<object>();
            javaURL.Add("http://www.oracle.com/technetwork/java/javase/downloads/java-archive-downloads-javase6-419409.html#jre-6u45-oth-JPR");
            javaURL.Add("http://www.oracle.com/technetwork/java/javase/archive-139210.html");

            while (!GotJava)
            {
                if (File.Exists(java64 + @"\java.exe"))
                {
                    bit64 = true;
                    GotJava = true;
                    break;
                }
                if (File.Exists(java86 + @"\java.exe"))
                {
                    bit64 = false;
                    GotJava = true;
                    break;
                }
                else
                {
                    GotJava = false;
                }
                Console.WriteLine("\n\n\n\n\nPLEASE INSTALL JAVA 1.6");
                Console.WriteLine("  ......I'll WAIT UNTIL YOU DO\n[PLEASE PRESS ENTER]");
                Console.WriteLine("\n\nNEED HELP? PLEASE SEE:");
                foreach (string url in javaURL)
                {
                    Console.WriteLine(url);
                }
                Console.ReadKey();
            }
        }

        //[[#AutoDECK]] ASYNC DOWNLOAD PROGRESS //#?
        public static void download_Progress(object sender, DownloadProgressChangedEventArgs e)
        {
            if (SHOW_RD)
            {
                Console.CursorVisible = false;
                int left = Console.CursorLeft;
                int x = Console.CursorLeft; //WRITE AT BOTTOM
                int y = Console.CursorTop; //WRITE AT BOTTOM
                Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1; //WRITE AT BOTTOM
                Console.ResetColor();
                //Alert.WEB("[[ RUNDECK DOWNLOAD ]]: " + e.BytesReceived.ToString() + " : " + e.ProgressPercentage.ToString() + "% ");
                Console.Write("[[ RUNDECK DOWNLOAD ]]: " + e.BytesReceived + ": " + e.ProgressPercentage + "% ");
                Console.SetCursorPosition(x, y); // Restore previous position
                Console.CursorLeft = left;
            }
        }
        //[[#AutoDECK]] ASYNC DOWNLOAD DONE
        public static void download_FileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Console.WriteLine("[AsyncCompletedEventArgs] Download completed!!!!!!!!!!!!!!!!!!!!!!!");
            GO = true;
        }

        public static void WebClient()
        {
            //[[#AutoDECK]] WEBCLIENT 
            string curFile = dir2 + @"\rundeck-launcher-2.3.2.jar";
            if (!File.Exists(curFile))
            {
                Directory.SetCurrentDirectory(dir2);
                //Console.WriteLine("[WEBCLIENT] Current directory: {0}", Directory.GetCurrentDirectory());
                string url = "http://dl.bintray.com/rundeck/rundeck-maven/rundeck-launcher-2.3.2.jar";
                string bin = "rundeck-launcher-2.3.2.jar";
                WebClient download = new WebClient();
                download.DownloadFileCompleted += new AsyncCompletedEventHandler(download_FileCompleted);
                download.DownloadProgressChanged += new DownloadProgressChangedEventHandler(download_Progress);
                download.DownloadFileAsync(new Uri(url), dir2 + @"\" + bin);
            }
            else
            {
                GO = true;
            }
        }


        public static void puttyProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            if(SHOW_PUTTY)
            {
            Console.CursorVisible = false;
            int left = Console.CursorLeft;
            int x = Console.CursorLeft; //WRITE AT BOTTOM
            int y = Console.CursorTop; //WRITE AT BOTTOM
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1; //WRITE AT BOTTOM
            Console.ResetColor();
            //Alert.WEB("[[ PUTTY DOWNLOAD ]]: " + e.BytesReceived.ToString() + " : " + e.ProgressPercentage.ToString() + "% ");
            Console.Write("[[ RUNDECK DOWNLOAD ]]: " + e.BytesReceived + ": " + e.ProgressPercentage + "% ");
            Console.SetCursorPosition(x, y); // Restore previous position
            Console.CursorLeft = left;
            }
        }

        public static void puttyDown(object sender, AsyncCompletedEventArgs e)
        {
            //Console.WriteLine("\n\n\nPUTTY DOWNLOAD COMPLETE [PLEASE PRESS ENTER]");
            PGO = true;
        }

        public static void GetPutty()
        {
            //IF x86/x64: 
            string putty64 = @"C:\Program Files (x86)\PuTTY";
            string putty86 = @"C:\Program Files\PuTTY";

            if (File.Exists(putty64 + @"\puttygen.exe"))
            {
                pbit64 = true;
                GotPutty = true;
            }
            else if (File.Exists(putty86 + @"\puttygen.exe"))
            {
                pbit64 = false;
                GotPutty = true;
            }
            else
            {
                GotPutty = false;
            }
            
            if (!GotPutty)
            {
                //[[#AutoDECK]] GetPutty 
                string curFile = dir3 + @"\putty-0.63-installer.exe";
                if (!File.Exists(curFile))
                {
                    Directory.SetCurrentDirectory(dir3);
                    //Console.WriteLine("[GetPutty] Current directory: {0}", Directory.GetCurrentDirectory());
                    string url = "http://the.earth.li/~sgtatham/putty/latest/x86/putty-0.63-installer.exe";
                    string bin = "putty-0.63-installer.exe";
                    WebClient download = new WebClient();
                    download.DownloadFileCompleted += new AsyncCompletedEventHandler(puttyDown);
                    download.DownloadProgressChanged += new DownloadProgressChangedEventHandler(puttyProgress);
                    download.DownloadFileAsync(new Uri(url), dir3 + @"\" + bin);
                }
                else
                {
                    PGO = true;
                }
            }
        }


        public static void installputty()
        {
            //PUTTY IS OPTIONAL (AS WE NOW MAKE OUR OWN RSA KEYS)
            string putty64 = @"C:\Program Files (x86)\PuTTY";
            string putty86 = @"C:\Program Files\PuTTY";

            if (File.Exists(putty64 + @"\puttygen.exe"))
            {
                pbit64 = true;
                GotPutty = true;
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\nPutty Installed: " + putty64 + @"\puttygen.exe");
            }
            else if (File.Exists(putty86 + @"\puttygen.exe"))
            {
                pbit64 = false;
                GotPutty = true;
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\nPutty Installed: " + putty86 + @"\puttygen.exe");
            }
            else
            {
                    GotPutty = false;
                    Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\nINSTALLING: (Optional) Putty");
                    Alert.DarkMagenta("[[ WAITING FOR PUTTY DOWNLOAD: \"Press Any Key To Cancel\" ]]");
                    while (!PGO) { }
                    //[[#AutoDECK]] START UP RUNDECK PROC w/FORK
                    //Directory.SetCurrentDirectory(dir3);
                    //Console.WriteLine("[START PUTTY INSTALL] Current directory: {0}", Directory.GetCurrentDirectory());
                    System.Diagnostics.Process _InstallP = new System.Diagnostics.Process();
                    _InstallP.StartInfo.FileName = dir3 + @"\putty-0.63-installer.exe";
                    _InstallP.StartInfo.Arguments = "/SUPPRESSMSGBOXES /NORESTART /SP- /VERYSILENT";
                    Thread putthread = new Thread(() => _InstallP.Start());
                    putthread.Name = "PUTTY_INSTALL_THREAD";
                    putthread.Start();
            }
        }


        public static void Rundeck_URL()
        {
            //CHECK FOR RUNDECK SERVER 
            Console.WriteLine("[ SEARCHING FOR RUNDECK SERVER PORT ]");
            bool portFound = false;
            do
            {
                int port = 4440; //Rundeck Server Port
                //Console.WriteLine("[ SEARCHING FOR RUNDECK SERVER PORT ]");
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] endPoints = properties.GetActiveTcpListeners();
                foreach (IPEndPoint e in endPoints)
                {
                    //Console.WriteLine(e.ToString());
                    if (e.ToString() == "0.0.0.0:" + port)
                    {
                        Console.WriteLine("....FOUND RUNDECK SERVER PORT (Attempting to start Rundeck URL in web browser!)");
                        portFound = true;
                        break;
                    }
                }
                Thread.Sleep(500);
            }
            while (!portFound);

            string RDurl = @"http://127.0.0.1:4440";
            System.Diagnostics.Process.Start(RDurl);
        }



    }//CLASS END


}//NAMESPACE END





//MAKE RSA KEYS
namespace dotNET_RSA_KEY
{
    class MAKE_RSA_KEY
    {
        //DIR VARS
        public static string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string dir0 = desktop + @"\AutoDECK";
        public static string dir1 = desktop + @"\AutoDECK\SSH_KEY";
        public static string dir2 = desktop + @"\AutoDECK\RUNDECK";
        public static string dir3 = desktop + @"\AutoDECK\BIN";

        //static string Main(string[] args)
        public static string MAKE_KEY()
        {

            //MAKE RSA KEY THEN EXPORT TO PUTTY KEY 

            // Create a new key pair on target CSP
            CspParameters myCspParams = new CspParameters();
            myCspParams.ProviderType = 1; // PROV_RSA_FULL 
            myCspParams.Flags = CspProviderFlags.UseArchivableKey;
            myCspParams.KeyNumber = (int)KeyNumber.Exchange;

            string Comment = "autodeck key";

            RSACryptoServiceProvider myRSA = new RSACryptoServiceProvider(2048, myCspParams); //KEY SIZE
            RSAParameters myRSAParams = myRSA.ExportParameters(true);
            //RAW KEY DATA
            //
            //myRSAParams.D  
            //myRSAParams.DP
            //myRSAParams.DQ
            //myRSAParams.Exponent
            //myRSAParams.InverseQ
            //myRSAParams.Modulus
            //myRSAParams.P
            //myRSAParams.Q

            //RSA KEY ToXmlString            
            //Console.WriteLine("\n\n\n\n\n\nTHIS IS THE TEXT KEY:\n{0}", myRSA.ToXmlString(true));

            //MAKE PUTTY PRV KEY

            //GET PUBLIC KEY
            var publicParameters = myRSA.ExportParameters(false);
            byte[] publicBuffer = new byte[3 + 7 + 4 + 1 + publicParameters.Exponent.Length + 4 + 1 + publicParameters.Modulus.Length + 1];

            using (var bw = new BinaryWriter(new MemoryStream(publicBuffer)))
            {
                bw.Write(new byte[] { 0x00, 0x00, 0x00 });
                bw.Write("ssh-rsa");
                PutPrefixed(bw, publicParameters.Exponent, true);
                PutPrefixed(bw, publicParameters.Modulus, true);
            }
            var publicBlob = System.Convert.ToBase64String(publicBuffer);

            //GET PRIVATE KEY
            var privateParameters = myRSA.ExportParameters(true);
            byte[] privateBuffer = new byte[4 + 1 + privateParameters.D.Length + 4 + 1 + privateParameters.P.Length + 4 + 1 + privateParameters.Q.Length + 4 + 1 + privateParameters.InverseQ.Length];

            using (var bw = new BinaryWriter(new MemoryStream(privateBuffer)))
            {
                PutPrefixed(bw, privateParameters.D, true);
                PutPrefixed(bw, privateParameters.P, true);
                PutPrefixed(bw, privateParameters.Q, true);
                PutPrefixed(bw, privateParameters.InverseQ, true);
            }
            var privateBlob = System.Convert.ToBase64String(privateBuffer);

            HMACSHA1 hmacsha1 = new HMACSHA1(new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes("putty-private-key-file-mac-key")));
            //byte[] bytesToHash = new byte[4 + 7 + 4 + 4 + 4 + this.Comment.Length + 4 + publicBuffer.Length + 4 + privateBuffer.Length];
            //byte[] bytesToHash = new byte[4 + 7 + 4 + 4 + 4 + 4 + publicBuffer.Length + 4 + privateBuffer.Length];
            byte[] bytesToHash = new byte[4 + 7 + 4 + 4 + 4 + Comment.Length + 4 + publicBuffer.Length + 4 + privateBuffer.Length];

            using (var bw = new BinaryWriter(new MemoryStream(bytesToHash)))
            {
                PutPrefixed(bw, Encoding.ASCII.GetBytes("ssh-rsa"));
                PutPrefixed(bw, Encoding.ASCII.GetBytes("none"));
                //PutPrefixed(bw, Encoding.ASCII.GetBytes(this.Comment));
                PutPrefixed(bw, Encoding.ASCII.GetBytes(Comment));
                PutPrefixed(bw, publicBuffer);
                PutPrefixed(bw, privateBuffer);
            }

            var hash = string.Join("", hmacsha1.ComputeHash(bytesToHash).Select(x => string.Format("{0:x2}", x)));

            var sb = new StringBuilder();
            sb.AppendLine("PuTTY-User-Key-File-2: ssh-rsa");
            sb.AppendLine("Encryption: none");
            sb.AppendLine("Comment: " + Comment);

            var publicLines = SpliceText(publicBlob, 64);
            sb.AppendLine("Public-Lines: " + publicLines.Length);
            foreach (var line in publicLines)
            {
                sb.AppendLine(line);
            }

            var privateLines = SpliceText(privateBlob, 64);
            sb.AppendLine("Private-Lines: " + privateLines.Length);
            foreach (var line in privateLines)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine("Private-MAC: " + hash);

            //Console.WriteLine("\n\n\n\n[ PUTTY PRV STRING ]: \n{0}", sb.ToString());
            
            //KEEP UNIX KEY IN FILE
            string ppk = dir1 + @"\AutoDECK_dsa.ppk";
            //Console.WriteLine("\n MAKING KEY PPK FILE");
            System.IO.StreamWriter ppkfile = new System.IO.StreamWriter(ppk);
            //System.IO.StreamWriter ppkfile = new System.IO.StreamWriter("autdeck_dsa.ppk");
            ppkfile.WriteLine(sb.ToString());
            ppkfile.Close();
            //Console.WriteLine(".........DONE MAKING PPK FILE");

            //[[#AutoDECK]] CONVERT putty public key
            string pattern1 = @"Public-Lines:";
            string pattern2 = @"Private-Lines:";
            StringBuilder sshkey = new StringBuilder();
            Regex rgx1 = new Regex("(?ix)" + pattern1);
            Regex rgx2 = new Regex("(?ix)" + pattern2);
            //String or StringBuilder to stream/memorystream
            string myString = sb.ToString();
            byte[] byteArray = Encoding.ASCII.GetBytes(myString);
            MemoryStream stream = new MemoryStream(byteArray);
            Boolean NEXT = false;
            Boolean DUMP = false;
            using (StreamReader sr = new StreamReader(stream))
            {
                string line;
                sshkey.Append("ssh-rsa ");
                while (( line = sr.ReadLine()) != null)
                {
                    if (rgx2.IsMatch(line))
                        DUMP = true;
                    if (DUMP) continue;
                    if (NEXT)
                        sshkey.Append(line);
                    if (rgx1.IsMatch(line))
                        NEXT = true;
                }
                sshkey.Append(" autodeck@pretend-machine.com");
            }
            //Console.Write(sshkey);
            //KEEP UNIX KEY IN FILE
            string keystringFile = dir1 + @"\sshkey-string.txt";
            //Console.WriteLine("\n MAKING KEY STRING FILE");
            System.IO.StreamWriter ksfile = new System.IO.StreamWriter(keystringFile);
            //System.IO.StreamWriter ksfile = new System.IO.StreamWriter("sshkey-string.txt");
            ksfile.WriteLine(sshkey);
            ksfile.Close();
            //Console.WriteLine(".........DONE MAKING KEY STRING FILE");
            return sshkey.ToString(); //RETURN STRING
        }

        private static void PutPrefixed(BinaryWriter bw, byte[] bytes, bool addLeadingNull = false)
        {
            bw.Write(BitConverter.GetBytes(bytes.Length + (addLeadingNull ? 1 : 0)).Reverse().ToArray());
            if (addLeadingNull)
                bw.Write(new byte[] { 0x00 });
            bw.Write(bytes);
        }

        private static string[] SpliceText(string text, int lineLength)
        {
            return Regex.Matches(text, ".{1," + lineLength + "}").Cast<Match>().Select(m => m.Value).ToArray();
        }

    }//CLASS END

}//NAMESPACE END

