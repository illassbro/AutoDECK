AutoDECK
========

AutoDECK is a automation tool for "RUNDECK(rundeck.org/)" made with .NET(C#)

<a href="http://www.youtube.com/watch?feature=player_embedded&v=FSgPDadcEek" target="_blank"><img src="http://img.youtube.com/vi/FSgPDadcEek/0.jpg" alt="AutoDECK" width="240" height="180" border="10" /></a>

#[[ This is AutoDECK ]]

## Description:

AutoDECK is a automation tool for "RUNDECK(rundeck.org)"; basically, it does the setup work for you from 5 to 5000 hosts! 


##Requirements:

.NET 4.0+ for AutoDECK (you likely already have this.)

http://www.microsoft.com/net

Java 1.6 for Rundeck (you likely DO NOT have this "version".)

http://www.oracle.com/technetwork/java/javase/downloads/java-archive-downloads-javase6-419409.html#jre-6u45-oth-JPR

http://www.oracle.com/technetwork/java/javase/archive-139210.html

#####(Sorry, Windows only for now...)

##Configuration:

None. This tool automates a basic install of Rundeck. 

(Plans to add more later)


##How to use it?:

Install Java 1.6; Download AutoDECK, Unzip, Dubbed Click...... As it runs, add your host list(IPs or hostnames) 
and password list in the notepad pop-ups and your are done. Enjoy your automated setup!

(Please see the video)


##Known Issues (AutoDECK):

#####"One thing to note is that the tool will try all listed passwords on every host until it finds one that works."
 
So if you have one password for every host it will be fast but if you have different passwords for each host 
try to make the two files (hostfile/passfile) match host to password and line for line; else, it will try passwords in the list until 
it finds one that works and this could take a long time depending on how many passwords you have.

#####EXAMPLE:
```
hostfile   passfile
--------   --------
host1      pass1
host2	   pass2
host3           <===== If no password is here expect that 
                       the tread will try both "pass1" & "pass2"
```

##Known Issues (Rundeck/jsch):

#####[[ "SSHProtocolFailure: Algorithm negotiation fail" -> RUNDECK ERROR ]]

This seems to be a known issue with SSH-1.99-OpenSSH_6.7 as it uses a newer "Diffie-Hellman group exchange" 
most of my test hosts using SSH-1.99-OpenSSH_6.4 and below work fine.

RUNDECK uses "jsch" for the SSH protocol and as it turns out "jsch" does not seem to support the newer SSH2 protocol key exchange yet.

http://www.jcraft.com/jsch/


