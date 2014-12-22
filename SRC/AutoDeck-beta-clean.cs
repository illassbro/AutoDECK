
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

        //DIR VARS
        public static string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string dir0 = desktop + @"\AutoDECK";
        public static string dir1 = desktop + @"\AutoDECK\SSH_KEY";
        public static string dir2 = desktop + @"\AutoDECK\RUNDECK";
        public static string dir3 = desktop + @"\AutoDECK\BIN";

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
        public static int TRUN = 0; //RUNNING THREAD COUNT

        //[[#AutoDECK]] THREAD POOL LOCK
        public static Object thisLock = new Object();
        public static Object wordLock = new Object();
        public static Object listLock = new Object();
        public static Object Lock_MakeXML = new Object();

        static void Main(string[] args)
        {

            // to avoid a possible InvalidOperationException. 
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MainThread";
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
            webthread.Name = "RUNDECK WEB THREAD";
            webthread.Start();

            //WEBCLIENT (DOWNLOAD PUTTY)
            Thread getpthread = new Thread(() => GetPutty());
            getpthread.Name = "GET PUTTY THREAD";
            getpthread.Start();

            //MAKE SSH KEYS
            //string ssh = MAKE_RSA_KEY.MAKE_KEY(); 
            //sshkey.Append(ssh);

            //MAKE SSH KEYS : THREAD
            string ssh = null; 
            Thread makekeythread = new Thread(() => ssh = MAKE_RSA_KEY.MAKE_KEY());
            makekeythread.Name = "MAKE RSA KEY THREAD";
            makekeythread.Start();
            //makekeythread.Join(); //RETURN SSH KEY LATER
            //sshkey.Append(ssh);


            //[[#AutoDECK]] GOT JAVA
            Boolean GotJava = false;
            string java64 = @"C:\Program Files (x86)\Java\jre6\bin\";
            string java86 = @"C:\Program Files\Java\jre6\bin\";

            List<object> javaURL = new List<object>();
            javaURL.Add("http://www.oracle.com/technetwork/java/javase/downloads/java-archive-downloads-javase6-419409.html#jre-6u45-oth-JPR");
            javaURL.Add("http://www.oracle.com/technetwork/java/javase/archive-139210.html");

            while (!GotJava)
            {
                //IF x86/x64: 
                try
                {
                    Directory.SetCurrentDirectory(java64);
                    Console.WriteLine("[GOT JAVA] Current directory: {0}", Directory.GetCurrentDirectory());
                }
                catch
                {
                    //NONE
                }

                
                if (File.Exists("java.exe"))
                {
                    bit64 = true;
                    GotJava = true;
                    break;
                }
                else
                {
                    try
                    {
                        Directory.SetCurrentDirectory(java86);
                        Console.WriteLine("[GOT JAVA] Current directory: {0}", Directory.GetCurrentDirectory());
                    }
                    catch
                    {
                        //NONE
                    }

                }
                if (File.Exists("java.exe"))
                {
                    bit64 = false;
                    GotJava = true;
                    break;
                }
                else
                {
                    GotJava = false;
                }
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\nPLEASE INSTALL JAVA 1.6");
                Console.WriteLine("  ......I'll WAIT UNTIL YOU DO\n[PLEASE PRESS ENTER]");
                Console.WriteLine("\n\nNEED HELP? PLEASE SEE:");
                foreach (string url in javaURL)
                {
                    Console.WriteLine(url);
                }
                Console.ReadKey();
            }


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
                notethread.Name = "NOTEPAD THREAD";
                notethread.Start();

                System.Diagnostics.Process PassList = new System.Diagnostics.Process();
                PassList.StartInfo.FileName = "notepad";
                PassList.StartInfo.Arguments = pFile;
                PassList.Start();

                PassList.WaitForExit();
                HostLlist.WaitForExit();
            }

            //PUTTY IS OPTIONAL (AS WE NOW MAKE OUR OWN RSA KEY)

            /*
            string putty64 = @"C:\Program Files (x86)\PuTTY\";
            string putty86 = @"C:\Program Files\PuTTY\";

            while (!GotPutty)
            {
                //IF x86/x64: 
                try
                {
                    Directory.SetCurrentDirectory(putty64);
                }
                catch
                {
                    //NONE
                }
                
                Console.WriteLine("[GOT Putty] Current directory: {0}", Directory.GetCurrentDirectory());
                if (File.Exists("puttygen.exe"))
                {
                    pbit64 = true;
                    GotPutty = true;
                    break;
                }
                else
                {
                    try
                    {
                        Directory.SetCurrentDirectory(putty86);
                        Console.WriteLine("[GOT Putty] Current directory: {0}", Directory.GetCurrentDirectory());
                    }
                    catch
                    {
                        //NONE
                    }

                }

                if (File.Exists("puttygen.exe"))
                {
                    pbit64 = false;
                    GotPutty = true;
                    break;
                }
                else
                {
                    GotPutty = false;

                    Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\nPLEASE INSTALL Putty");
                    Console.WriteLine("  ......I'll WAIT UNTIL YOU DO\n[PLEASE PRESS ENTER]");
                    
                    while (!PGO) { }

                    //[[#AutoDECK]] START UP RUNDECK PROC w/FORK
                    Directory.SetCurrentDirectory(dir3);
                    //Console.WriteLine("[START PUTTY INSTALL] Current directory: {0}", Directory.GetCurrentDirectory());
                    System.Diagnostics.Process _InstallP = new System.Diagnostics.Process();
                    _InstallP.StartInfo.FileName = "putty-0.63-installer.exe";
                    Thread putthread = new Thread(() => _InstallP.Start());
                    putthread.Name = "PUTTY INSTALL THREAD";
                    putthread.Start();
                    
                }

                if (GotPutty) break;
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\nPLEASE INSTALL Putty");
                Console.WriteLine("  ......I'll WAIT UNTIL YOU DO\n[PLEASE PRESS ENTER]");
                Console.ReadKey();
            }


            //[[#AutoDECK]] OPEN PuttyGen to make KEYS
            prvkeyFile = dir1 + @"\id_dsa.ppk";
            pubkeyFile = dir1 + @"\id_dsa.pub";

            List<object> keyGenURL = new List<object>();
            keyGenURL.Add("http://wiki.joyent.com/wiki/display/jpc2/Manually+Generating+Your+SSH+Key+in+Windows");
            keyGenURL.Add("https://www.feralhosting.com/faq/view?question=13");
            keyGenURL.Add("http://www.tecmint.com/ssh-passwordless-login-with-putty/");

            if (!File.Exists(prvkeyFile) && !File.Exists(pubkeyFile))
            {
                System.Diagnostics.Process PuttyGen = new System.Diagnostics.Process();
                PuttyGen.StartInfo.FileName = "puttygen";
                PuttyGen.StartInfo.Arguments = "";
                PuttyGen.StartInfo.WorkingDirectory = dir1;
                Thread puttythread = new Thread(() => PuttyGen.Start());
                puttythread.Name = "PUTTY THREAD";
                puttythread.Start();
            }

            while (!File.Exists(prvkeyFile) && !File.Exists(pubkeyFile))
            {
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n[PLEASE LOAD/CREATE SSH KEY FILES]\nPUT INTO THIS DIR:\n({2})\n\n  -PRIVATE KEY: {0} \n  -PUBLIC KEY: {1} \n", prvkeyFile, pubkeyFile, dir1);
                Console.WriteLine("   ......I'll WAIT UNTIL YOU DO\n[PLEASE PRESS ENTER]");
                Console.WriteLine("NOTE: DO NOT add a password to your key!!!");
                Console.WriteLine("\n\nNEED HELP? PLEASE SEE:");
                foreach (string url in keyGenURL)
                {
                    Console.WriteLine(url);
                }
                Console.ReadKey();
            }


            //[[#AutoDECK]] CONVERT putty public key
            string pattern1 = @"begin";
            string pattern2 = @"end";
            string pattern3 = @"comment:";
            Regex rgx1 = new Regex("(?ix)" + pattern1);
            Regex rgx2 = new Regex("(?ix)" + pattern2);
            Regex rgx3 = new Regex("(?ix)" + pattern3);
            //
            using (StreamReader sr = new StreamReader(pubkeyFile))
            {
                string line;
                sshkey.Append("ssh-rsa ");
                while ((line = sr.ReadLine()) != null)
                {
                    if (rgx1.IsMatch(line)) continue;
                    if (rgx2.IsMatch(line)) continue;
                    if (rgx3.IsMatch(line)) continue;
                    string s = line.TrimEnd('\r', '\n');
                    sshkey.Append(s);
                }
                sshkey.Append(" autodeck@pretend-machine.com");
            }
            Console.Write(sshkey);


            //KEEP UNIX KEY IN FILE
            string keystringFile = dir1 + @"\sshkey-string.txt";
            Console.WriteLine("\n MAKING KEY STRING FILE");
            System.IO.StreamWriter ksfile = new System.IO.StreamWriter(keystringFile);
            ksfile.WriteLine(sshkey);
            ksfile.Close();
            Console.WriteLine(".........DONE MAKING KEY STRING FILE");

            */


            //[[#AutoDECK]] LOAD LISTS
            Directory.SetCurrentDirectory(dir0);
            Console.WriteLine("[GET HOST INFO] Current directory: {0}", Directory.GetCurrentDirectory());
            string HOSTFILE = "hostfile.txt";
            var hostlist = File.ReadAllLines(HOSTFILE);
            List<object> hosts = new List<object>(hostlist); //HOST LIST
            //hosts.Add("14.0.0.126");


            Directory.SetCurrentDirectory(dir0);
            Console.WriteLine("[GET PASSWORD INFO] Current directory: {0}", Directory.GetCurrentDirectory());
            string PASSFILE = "passfile.txt";
            var passlist = File.ReadAllLines(PASSFILE);
            foreach (var s in passlist) pass.Add(s); //PASSWD LIST 
            //pass.Add("passwd");

            List<object> user = new List<object>(); //USER LIST [NOT USED FOR NOW]
            user.Add("root");


            makekeythread.Join(); //RETURN SSHKEY HERE
            sshkey.Append(ssh);


            //SSH CLIENT CONF THREADS

            //These Threads MAKE/UPDATE RUNDECK resource.xml

            //[[#AutoDECK]] (My) THREAD POOL : RUN NO MORE THAN YOU NEED TO
            int t = 0;
            foreach (string host in hosts) //MAKE THREAD COUNT
                              t++;
            Thread[] threads = new Thread[t]; //THREAD ARRAY
            Console.WriteLine("QUEUED UP [" + t + "] THREADS.."); //SHOW HOW MANY THREADS

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
                            TMAX += 2; 
                            OD = 0; 
                            Console.WriteLine("ADDED MORE THREADS#: " + TMAX); 
                        }
                    }
                        
                } //BLOCK THREADS AT MAX
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("RUNNING THREAD #: " + i);
                Console.ResetColor();
                threads[i] = new Thread(() => tssh(host));
                threads[i].Name = i.ToString(); //SET THREAD NUM
                threads[i].Start();
                i++;
            }


            //MAKE RUNDECK CONFIG FILES
            Thread cfgthread = new Thread(() => MakeCFG());
            cfgthread.Name = "RUNDECK CFG THREAD";
            cfgthread.Start();


            if ((!GO) && (!COMPLETE))
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\n[[WAITING FOR DOWNLOAD]]");
                Console.ResetColor();
                //Console.WriteLine("\n\n[[SEE: http://rundeck.org/docs/]]");
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


            //WAIT FOR SSH CLIENT CONF THREADS
            int tha = t - 1;
            int thd = t - 1;
            for (; tha > 0; tha--)
            {
                //Console.WriteLine("THREAD DEAD: " + tha);
                if (!threads[tha].IsAlive)
                {
                    thd--;
                }
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                if (threads[tha].IsAlive)
                    Console.WriteLine("\n[[WAITING ON SSH CLIENT CONF THREAD]]: {0}", threads[tha].Name);
                Console.ResetColor();
            }

            //WAIT FOR SSH CLIENT CONF THREADS
            int TH = 0; //THREAD COUNT
            foreach (string host in hosts)
            {
                threads[TH].Join();
                TH++;
            }


            //END           
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nALL DONE\nGOTO: http://127.0.0.1:4440\nUserName: admin\nPassWord: admin\n[[PRESS ENTER (to exit)]]");
            Console.WriteLine("\n\n[[SEE: http://rundeck.org/docs/]]");
            Console.WriteLine("(Optional) Install Putty: {0}", dir3);
            Console.ReadKey();




        }//METHODS


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
                myword.Add(new goodword { ghost = host, guser = user, gpassword = pw }); //UPDATE PASSWORD LIST
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
                else if (TMAX > 51)
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
                    //int idx = mypass.IndexOf(p); //INDEX OF ITEM
                    int idx = cp;
                    cp++;
                    int tc = Convert.ToInt16(Thread.CurrentThread.Name); //USE THREAD NAME AS INDEX
                    //Console.WriteLine("THREAD{2}: {0} == {1}?", idx, tc, Thread.CurrentThread.Name); //DEBUG VIEW
                    if (idx == tc)
                    {
                        newmypass.RemoveAt(idx); //SHUFFLE: Remove Item Here
                        //newmypass.Insert(0, "ADDED: " + p); //TESTING
                        newmypass.Insert(0,p); //ADD Item to top of list
                        break;
                    }
                }
                usemypass = newmypass; //LAST COPY

                /*
                //TEST NEW LIST
                int osize = newmypass.Count;
                int isize = mypass.Count;
                //Console.WriteLine("SIZE OF LIST: {0} = {1}?", isize, osize); //DEBUG VIEW
                foreach (string i in usemypass)
                {
                    Console.WriteLine("{1} NEW LIST: {0}", i, Thread.CurrentThread.Name); //DEBUG VIEW
                    break;
                }
                */
                List<object> sh = usemypass;
                return sh;
            }
        }


        //[[#AutoDECK]] SSH THREADS
        public static void tssh(string host)
        {
            IsRUN(); //UPDATE POOL COUNTER

            List<object> thepass = new List<object>();
            thepass = shuffle(); //DO THE SHUFFLE

            try
            {
                Console.WriteLine(">> {0} START: SSH TO HOST: {1} <<", Thread.CurrentThread.Name, host);
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
                                //Console.WriteLine("[I AM DONE]: {0}", Thread.CurrentThread.Name);
                                
                            }
                            //catch (Exception con)
                            catch
                            {
                                //Console.WriteLine("SSH CONNECTION FAILURE: [BAD PASSWORD?  " + pw + "] [HOST: " + host + "]" + con);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("THREAD# {0} SSH CONNECTION FAILURE: [HOST: {1}] (BAD PASSWORD?)", Thread.CurrentThread.Name, host);
                                //Console.WriteLine("{0} SSH CONNECTION FAILURE: [BAD PASSWORD? {2}] [HOST: {1}]", Thread.CurrentThread.Name, host, pw);
                                Console.ResetColor();
                                continue; //IF CONNECTION FAILS NEXT LOOP
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
                            break; //IF WORKS BREAK LOOP
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} FAILED: @HOST: {1}\n{3}", Thread.CurrentThread.Name, host, pw, e.ToString());
                        Console.ResetColor();
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

            //Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[[ {0} COMPLETED @HOST: {1} ]]", Thread.CurrentThread.Name, host);
            Console.ResetColor();
            IsNotRUN();//UPDATE POOL COUNTER
        }


        public static void MakeCFG()
        {
            //while (TRUN != 0){}; //WAIT FOR THREAD POOL
            while (!GO){}; //WAIT FOR GO

            string autoETC = dir2 + @"\projects\AutoDECK\etc\";
            Directory.CreateDirectory(autoETC);
            Directory.SetCurrentDirectory(autoETC);
            Console.WriteLine("Current directory: {0}", Directory.GetCurrentDirectory());

            //GEN STRINGS 
            string pattern = @"\\";
            string replacement = @"\\";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(dir2 + @"\projects\AutoDECK\etc\resources.xml", replacement);
            string rdeck = result.Replace(":", @"\:");
            Console.WriteLine(rdeck);

            string npattern = @"\\";
            string nreplacement = @"\\";
            Regex nrgx = new Regex(npattern);
            string nresult = nrgx.Replace(dir1 + @"\AutoDECK_dsa.ppk", nreplacement);
            string rdkey = nresult.Replace(":", @"\:");
            Console.WriteLine(rdkey);

            //[[#AutoDECK]] WRITE RUNDECK project.properties
            Console.WriteLine("MAKING project.properties FILE");
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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[MAKING/UPDATING RUNDECK XML FILE] I AM THREAD: {0}", Thread.CurrentThread.Name);
                Console.ResetColor();

                foreach (goodword w in aword) {
                    addPASS(w.ghost, w.guser, w.gpassword);
                }

                string autoETC = dir2 + @"\projects\AutoDECK\etc\";
                Directory.CreateDirectory(autoETC);
                //Console.WriteLine("MAKING RUNDECK XML FILE");

                //XmlTextWriter xwr = new XmlTextWriter(autoETC + "resources.xml", Encoding.UTF8);

                XmlTextWriter xwr;
                string xfilename = autoETC + "resources.xml";

                if (File.Exists(xfilename))
                {
                    XDocument xDocument = XDocument.Load(xfilename);
                    XElement root = xDocument.Element("project");
                    IEnumerable<XElement> rows = root.Descendants("node");
                    XElement firstRow = rows.First(); //START AT TOP (FASTER)
                    //XElement lastRow = rows.Last(); //START AT BOTTOM

                    //foreach (goodword w in myword)
                    foreach (goodword w in aword)
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
                Console.WriteLine(".....DONE MAKING/UPDATING RUNDECK XML FILE");
                //COMPLETE = true;
            }
        }


        //[[#AutoDECK]] ASYNC DOWNLOAD PROGRESS
        public static void download_Progress(object sender, DownloadProgressChangedEventArgs e)
        {
            //Console.WriteLine("BYTES READ: " + e.BytesReceived + ": " + e.ProgressPercentage + "%");
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
                Console.WriteLine("[WEBCLIENT] Current directory: {0}", Directory.GetCurrentDirectory());

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
            //Console.WriteLine("BYTES READ: " + e.BytesReceived + ": " + e.ProgressPercentage + "%");
        }

        public static void puttyDown(object sender, AsyncCompletedEventArgs e)
        {
            //Console.WriteLine("\n\n\nPUTTY DOWNLOAD COMPLETE [PLEASE PRESS ENTER]");
            PGO = true;
        }

        public static void GetPutty()
        {
            //IF x86/x64: 
            string putty64 = @"C:\Program Files (x86)\PuTTY\";
            string putty86 = @"C:\Program Files\PuTTY\";
            try
            {
                Directory.SetCurrentDirectory(putty64);
            }
            catch
            {
                //NONE
            }
            //
            //Console.WriteLine("[GOT Putty] Current directory: {0}", Directory.GetCurrentDirectory());
            if (File.Exists("puttygen.exe"))
            {
                pbit64 = true;
                GotPutty = true;
            }
            else
            {
                try
                {
                    Directory.SetCurrentDirectory(putty86);
                    Console.WriteLine("[GOT Putty] Current directory: {0}", Directory.GetCurrentDirectory());
                }
                catch
                {
                    //NONE
                }
                //
                if (File.Exists("puttygen.exe"))
                {
                    pbit64 = false;
                    GotPutty = true;
                }
                else
                {
                    GotPutty = false;
                }
            }


            if (!GotPutty)
            {
                //[[#AutoDECK]] GetPutty 
                string curFile = dir3 + @"\putty-0.63-installer.exe";
                if (!File.Exists(curFile))
                {
                    Directory.SetCurrentDirectory(dir3);
                    Console.WriteLine("[GetPutty] Current directory: {0}", Directory.GetCurrentDirectory());

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
            Console.WriteLine("\n MAKING KEY PPK FILE");
            System.IO.StreamWriter ppkfile = new System.IO.StreamWriter(ppk);
            //System.IO.StreamWriter ppkfile = new System.IO.StreamWriter("autdeck_dsa.ppk");
            ppkfile.WriteLine(sb.ToString());
            ppkfile.Close();
            Console.WriteLine(".........DONE MAKING PPK FILE");

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
            Console.Write(sshkey);

            //KEEP UNIX KEY IN FILE
            string keystringFile = dir1 + @"\sshkey-string.txt";
            Console.WriteLine("\n MAKING KEY STRING FILE");
            System.IO.StreamWriter ksfile = new System.IO.StreamWriter(keystringFile);
            //System.IO.StreamWriter ksfile = new System.IO.StreamWriter("sshkey-string.txt");
            ksfile.WriteLine(sshkey);
            ksfile.Close();
            Console.WriteLine(".........DONE MAKING KEY STRING FILE");

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

