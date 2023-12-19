// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

//using System;
using System.Diagnostics;
using System.Timers;
//using System.IO;

public class Program
{

//Global Variables
public static StreamWriter logStreamWriter ;

    //initialized with default values....
public static string srcPath = "./srcFolder/";
public static string dstPath = "./dstFolder/";
public static string logPath = "./Logs/";


    //CommandLine Arguements Index=0 will be Source Directory, Index=1 will be Replica Directory
    //---------------------- Index=2 will be Timer value (miliseconds), Index=3 will be Log File location
	public static void Main(string[] cmdLineArgs)
	{

        cmdLineArgs.Append(srcPath);
        cmdLineArgs.Append(dstPath);
        cmdLineArgs.Append(logPath);
        cmdLineArgs.Append("12300");

        try{

            if( cmdLineArgs != null && cmdLineArgs.Length > 3  && cmdLineArgs[2] != null && Int32.TryParse(cmdLineArgs[2], out Int32 TimerValue) ){
//{Int32 TimerValue = 12300;
                

                //check Log File path exists...
                if( Directory.Exists(logPath) && Directory.Exists(srcPath) && Directory.Exists(dstPath) ){

                    //----------------
                    // Create a timer and set a two second interval.
                    System.Timers.Timer timerToTrigger = new System.Timers.Timer();
                    timerToTrigger.Interval = TimerValue ;

                    // Hook up the Elapsed event for the timer. 
                    timerToTrigger.Elapsed += OneWayFolderSync;

                    // Have the timer fire repeated events (true is the default)
                    timerToTrigger.AutoReset = true;

                    // Start the timer
                    timerToTrigger.Enabled = true;

                    Console.WriteLine("Press the Enter key to exit the program at any time... ");
                    Console.ReadLine();
                    //TimedEvent(srcPath, dstPath, logPath);


                    
                }// if Directories Exist

            } //if cmdLineArguements
           
        }//try
        catch(IOException ex){
            Console.WriteLine("Log file path does not exist." + ex.InnerException);
        }

	}

    private static void OneWayFolderSync(object? sender, ElapsedEventArgs e)
    {
        try{

            //check Log File path exists...
            if( Directory.Exists(srcPath) && Directory.Exists(dstPath) && Directory.Exists(logPath) ){

                //create Log file
                string LogFileName = logPath + DateTime.Now.TimeOfDay.ToString() + ".log" ;
                logStreamWriter = new StreamWriter(LogFileName);

                logStreamWriter.Write(LogsConsole("Commandline Arguement 0" + srcPath));
                logStreamWriter.Write(LogsConsole("Commandline Arguement 1" + dstPath));
                logStreamWriter.Write(LogsConsole("Commandline Arguement 3" + logPath));
             
                Stopwatch stpWatch = new Stopwatch();
                stpWatch.Start();

    
                OneWayFolderSyncExecution(new DirectoryInfo(srcPath), new DirectoryInfo(dstPath), logPath);

                stpWatch.Stop();

                string elapsedTime = stpWatch.Elapsed.ToString();
                logStreamWriter.Write(LogsConsole("Total execution time " + elapsedTime + "Miliseconds"));
                logStreamWriter.Close();
            }//IF DIRECTORIES EXIST

        }//try
        catch(IOException ex){
            Console.WriteLine("Log file path does not exist." + ex.InnerException);
        }
    }

    public static string LogsConsole(string strValue){
                Console.WriteLine("\n" + strValue);
                return "\n" + DateTime.Now.ToString() + " " + strValue;
    }

    public static void OneWayFolderSyncExecution(DirectoryInfo srcInfo, DirectoryInfo dstInfo, string logPath){

        //Copy Files....
        try{

            List<string> dstFilesToDelete = Directory.GetFiles(dstInfo.FullName).ToList();
            FileInfo[] scrFilesList = srcInfo.GetFiles();

            foreach(FileInfo file in scrFilesList){

                string str = dstInfo+file.Name;
                logStreamWriter.Write(LogsConsole("Preparing to copy file ...  " +str ));
                
                string PathFile = Path.Combine( dstInfo.FullName, file.Name);

                if(dstFilesToDelete.Contains(PathFile)){

                    //File to be updated and should not be marked as to be deleted...
                    dstFilesToDelete.Remove(PathFile);
                }

                if (new FileInfo(PathFile).Exists) {

                    str = dstInfo+file.Name;
                    logStreamWriter.Write(LogsConsole("Overwriting file ...  " + str));
                    file.CopyTo(  PathFile, true );
                }
                else {

                    str = dstInfo+file.Name;
                    logStreamWriter.Write(LogsConsole("Copying new file ... " + str));
                    file.CopyTo(  PathFile);
                }

            }// for each file copy

            if(dstFilesToDelete.Count > 0 ){

                foreach(string dstDeleteFile in dstFilesToDelete){

                    //check if the path value is not null
                    if( dstDeleteFile != null ){  

                        //file to be deleted
                        logStreamWriter.Write(LogsConsole("Deleting destination file ... " + dstDeleteFile));
                        new FileInfo(dstDeleteFile).Delete();
                    }
                }
            }

        }
        catch (IOException ex){
            string str = ex.InnerException.ToString();
            logStreamWriter.Write(LogsConsole("Directory operations interrupted ... " + str));
        }
        

        //Copy Sub-Directories
        try{

            List<string> dstDirectoriesToDelete = Directory.GetDirectories(dstInfo.FullName).ToList();
            DirectoryInfo[] srcDirectories = srcInfo.GetDirectories();

            foreach(DirectoryInfo srcSubDir in srcDirectories){

                string str = dstInfo+srcSubDir.Name;
                logStreamWriter.Write(LogsConsole("Preparing to copy Directory ... " + str));
                
                DirectoryInfo dstSubDir = new DirectoryInfo(Path.Combine(dstInfo.FullName, srcSubDir.Name));

                if(dstDirectoriesToDelete.Contains(dstSubDir.ToString())){

                    //Directory to be updated
                    dstDirectoriesToDelete.Remove(dstSubDir.ToString());
                }
            
                str = srcSubDir.Name;
                logStreamWriter.Write(LogsConsole("Preparing to copy SubDirectory ... " + str));

                if(dstSubDir.Exists){

                    logStreamWriter.Write(LogsConsole("Sub-Directory already Exists ... " + dstSubDir));
                }
                else {

                    logStreamWriter.Write(LogsConsole("Creating New SubDirectory ... " + dstSubDir));
                    dstSubDir.Create();
                }

                //recurrsive call 
                OneWayFolderSyncExecution(srcSubDir, dstSubDir, logPath);
            }

            if(dstDirectoriesToDelete.Count > 0){
                foreach(string dstDeleteDirectory in dstDirectoriesToDelete){

                    //check if the path value is not null
                    if( dstDeleteDirectory != null ){  

                        //file to be deleted
                        logStreamWriter.Write(LogsConsole("Deleting destination directory (recurrsive operation) ... " + dstDeleteDirectory));
                        Directory.Delete(dstDeleteDirectory, true);         //recurrsive delete...
                    }
                }
            }

        }
        catch (IOException ex){
            string str = ex.InnerException.ToString();
            logStreamWriter.Write(LogsConsole("Directory operations interrupted ... " + str));
        }

    }
}