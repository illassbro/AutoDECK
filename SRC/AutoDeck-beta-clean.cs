
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Renci.SshNet;

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
        public static int TMAX = 5; //THREAD MAX
        public static int TRUN = 0; //RUNNING THREAD COUNT

        //[[#AutoDECK]] THREAD POOL LOCK
        public static Object thisLock = new Object();
        public static Object wordLock = new Object();
        public static Object listLock = new Object();

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


            //[[#AutoDECK]] GOT JAVA
            Boolean GotJava = false;
            Boolean bit64 = false;
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

            Console.WriteLine("MAKING RUNDECK BAT FILE");
            System.IO.StreamWriter rdfile = new System.IO.StreamWriter(@"RUNDECK-START.bat");
            StringBuilder rdeck = new StringBuilder();
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
                xpfile.WriteLine("123456");
                xpfile.Close();

                System.IO.StreamWriter xhfile = new System.IO.StreamWriter(hFile);
                xhfile.WriteLine("REMOVE THESE LINES: [ PUT HOST LIST HERE ]");
                xhfile.WriteLine("hostname1");
                xhfile.WriteLine("hostname2");
                xhfile.WriteLine("hostname3");
                xhfile.WriteLine("or-ipaddress");
                xhfile.WriteLine("14.0.0.126");
                xhfile.WriteLine("14.0.0.138");
                xhfile.WriteLine("14.0.0.131");
                xhfile.WriteLine("14.0.0.103");
                xhfile.WriteLine("14.0.0.141");
                xhfile.WriteLine("14.0.0.130");
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

                    //WEBCLIENT (DOWNLOAD PUTTY)
                    Thread getpthread = new Thread(() => _GetPutty());
                    getpthread.Name = "GET PUTTY THREAD";
                    getpthread.Start();
                    
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

            //[[#AutoDECK]] (DONE) FOR NOW JUST LOAD LISTS
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


            //[[#AutoDECK]] (My) THREAD POOL : RUN NO MORE THAN MAX(TMAX)
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
                            TMAX += 2 ; 
                            OD = 0; 
                            Console.WriteLine("ADDED MORE THREADS#: " + TMAX); 
                        }
                    }
                        
                } //BLOCK THREADS AT MAX
                Console.WriteLine("RUNNING THREAD #: " + i);
                threads[i] = new Thread(() => _tssh(host));
                threads[i].Name = i.ToString(); //SET THREAD NUM
                threads[i].Start();
                i++;
            }


            //MAKE SURE ALL THREADS IN POOL ARE DEAD
            while(true)
            { 
                int tha = t -1;
                int thd = t -1;
                for ( ; tha > 0 ; tha-- )
                {
                    //Console.WriteLine("THREAD DEAD: " + tha);
                        if (!threads[tha].IsAlive)
                        {
                            thd--;
                        }
                }
                if(thd <= 0)
                    break;
            }



            //MAKE RUNDECK resource.xml
            Thread makethread = new Thread(() => _MakeXML());
            makethread.Name = "RUNDECK MAKE THREAD";
            makethread.Start();

            //WEBCLIENT (DOWNLOAD RUNDECK)
            Thread webthread = new Thread(() => _WebClient());
            webthread.Name = "RUNDECK WEB THREAD";
            webthread.Start();

            //[[#AutoDECK]] START UP RUNDECK PROC w/FORK
            while (!GO) { }
            while (!COMPLETE) { }
            Directory.SetCurrentDirectory(dir2);
            Console.WriteLine("[START RUNDECK] Current directory: {0}", Directory.GetCurrentDirectory());
            System.Diagnostics.Process _RunDeck = new System.Diagnostics.Process();
            _RunDeck.StartInfo.FileName = "cmd";
            _RunDeck.StartInfo.Arguments = "/c START cmd /T:6b  /k " + rdeck;
            Thread rundeckthread = new Thread(() => _RunDeck.Start());
            rundeckthread.Name = "RUNDECK THREAD";
            rundeckthread.Start();
        
            //END           
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nALL DONE\nGOTO: http://127.0.0.1:4440\nUserName: admin\nPassWord: admin\n[[PRESS ENTER (to exit)]]");
            Console.WriteLine("\n\n[[SEE: http://rundeck.org/docs/]]");
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


        public static void addPASS(object host, object user, object pw)
        {
            lock (wordLock)
            {
                myword.Add(new goodword { ghost = host, guser = user, gpassword = pw }); //UPDATE PASSWORD LIST
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
        public static void _tssh(string host)
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
                                var jim = new SshClient(host, user, pw);
                                client.Connect(); //TRY CONNECTION
                                //Console.WriteLine("{0} CONNECTED: [GOOD PASSWORD: {2}] [HOST: {1}]", Thread.CurrentThread.Name, host, pw);
                                addPASS(host, user, pw);

                                
                            }
                            //catch (Exception con)
                            catch
                            {
                                //Console.WriteLine("SSH CONNECTION FAILURE: [BAD PASSWORD?  " + pw + "] [HOST: " + host + "]" + con);
                                Console.WriteLine("{0} SSH CONNECTION FAILURE: [BAD PASSWORD? {2}] [HOST: {1}]", Thread.CurrentThread.Name, host, pw);
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
                        Console.WriteLine("{0} FAILED: @HOST: {1}\n{3}", Thread.CurrentThread.Name, host, pw, e.ToString());
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

            Console.WriteLine("[[ {0} COMPLETED @HOST: {1} ]]", Thread.CurrentThread.Name, host);
            IsNotRUN();//UPDATE POOL COUNTER
        }


        public static void _MakeXML()
        {
            while (TRUN != 0){}; //WAIT FOR THREAD POOL
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
            string nresult = nrgx.Replace(dir1 + @"\id_dsa.ppk", nreplacement);
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

            //[[#AutoDECK]] MAKE XML FILE
            Directory.SetCurrentDirectory(autoETC);
            Console.WriteLine("Current directory: {0}", Directory.GetCurrentDirectory());

            Console.WriteLine("MAKING RUNDECK XML FILE");
            XmlTextWriter xwr = new XmlTextWriter("resources.xml", Encoding.UTF8);
            xwr.Formatting = Formatting.Indented;
            xwr.WriteStartDocument();
            xwr.WriteStartElement("project"); //<project>
            foreach (goodword w in myword)
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
            Console.WriteLine(".....DONE MAKING RUNDECK XML FILE");
            COMPLETE = true;
        }

        //[[#AutoDECK]] ASYNC DOWNLOAD PROGRESS
        public static void download_Progress(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("BYTES READ: " + e.BytesReceived + ": " + e.ProgressPercentage + "%");
        }
        //[[#AutoDECK]] ASYNC DOWNLOAD DONE
        public static void download_FileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("[AsyncCompletedEventArgs] Download completed!!!!!!!!!!!!!!!!!!!!!!!");
            GO = true;
        }

        public static void _WebClient()
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
                download.DownloadFileAsync(new Uri(url), bin);
            }
            else
            {
                GO = true;
            }
        }


        public static void puttyProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("BYTES READ: " + e.BytesReceived + ": " + e.ProgressPercentage + "%");
        }

        public static void puttyDown(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("\n\n\nPUTTY DOWNLOAD COMPLETE [PLEASE PRESS ENTER]");
            PGO = true;
        }

        public static void _GetPutty()
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
                download.DownloadFileAsync(new Uri(url), bin);
            }
            else
            {
                PGO = true;
            }
        }



    }//CLASS END


}//NAMESPACE END
