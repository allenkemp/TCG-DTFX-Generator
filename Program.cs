//Data we need to set
String sourceLibrary = "IESFILE";
String hostName = "127.0.0.1";
String outputPath = "";
String acsPath = "";
String userID = "defaultUser";
String userPass = "";

List<String> outputFileNames = new List<String>(){
  "billing",
  "drivers",
  "freight",
  "gldat",
  "lmneg",
  "pyearnh",
  "tcgcity",
  "tcgcust",
  "tcglinh",
  "tcgmile"
};

//Gather Input from User

//Get IBM hostname / IP Address
System.Console.Write("Please provide hostname or IP address of IBM Server where TCG source files are located: ");
hostName = System.Console.ReadLine();

//Get User Info
System.Console.Write(
@"
Please provide the username to be used for connecting to your IBM server.
User should have correct privledges to be able to access and transfer files on the server.
");
userID = System.Console.ReadLine();

//Get Password if user chooses to store. Warn that if they don't, they will be prompted at each file transfer
System.Console.Write(
@"
If you wish to store the password for the user profile used to conduct file transfers, enter it here.
Please note, if you choose to not store the password, you will be prompted for it prior to each
source file transferring. The password is stored in plain text in the generated script files.
");
userPass = System.Console.ReadLine();

//Get Path to ACS
System.Console.Write(
@"
Please provide the complete path to the acslaunch executable. By default,
the install location for this is:
C:\Users\<<Your User>>\IBM\ClientSolutions\Start_Programs\Windows_x86-64\acslaunch_win-64.exe
The easiest way to provide this information is to right click on the Access Client Solutions icon on
your desktop, select 'Properties' and then copy and paste the information in the 'target' field:
");
acsPath = System.Console.ReadLine();

//Get Library for files
System.Console.Write("Please provide the Library Name where TCG source file are located: ");
System.Console.ReadLine().ToUpper();

//Set output path
System.Console.Write(
@"
Please Specify the location to store the files transferred from your IBM server.
If you do not provide a location here (just press enter),
 files will be downloaded to the directory where this program resides:
");
outputPath = System.Console.ReadLine();

//Check if output path provided by user is blank, if it is, replace
//with the current directory of the assembly.
outputPath = outputPath.Equals("") ? System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) : outputPath;

//check if the path is terminated with a '/'.  If it isn't, add one.
outputPath = outputPath.EndsWith("/") ? outputPath : outputPath + "/";

//Output Summary of information for user:


//Create dtfx files
foreach (string item in outputFileNames)
{
  
//Interpolated String Literal for output
string dtfxTemplate = 
$@"[DataTransferFrom]
DataTransferVersion=1.0
[HostInfo]
Database=*SYSBAS
HostFile={sourceLibrary.ToUpper()}/{item.ToUpper()}
HostName={hostName}
[ClientInfo]
CrtOpt=1
FDFFile={outputPath}{item}.fdfx
OutputDevice=2
SaveFDF=1
ClientFileType=1
ClientFile={outputPath}{item}.txt
TruncateTrailingSpaces=1
PadNumericFields=1
PadNumericFieldsWithLeadingSpaces=1
FileEncoding=UTF-8
[SQL]
EnableGroup=0
GroupBy=
Having=
JoinBy=
MissingFields=0
OrderBy=
SQLSelect=
Select=*
Where=
[Options]
DateFmt=MDY
DateSep=[/]
DecimalSep=.
IgnoreDecErr=1
Lang=0
LangID=
SortSeq=0
SortTable=
TimeFmt=HMS
TimeSep=[:]
[HTML]
AutoSize=0
AutoSizeKB=128
CapAlign=0
CapIncNum=0
CapSize=6
Caption=
CellAlignN=0
CellAlignT=0
CellSize=6
CellWrap=1
Charset=
ConvInd=0
DateTimeLoc=0
IncDateTime=0
OverWrite=1
RowAlignGenH=0
RowAlignGenV=0
RowAlignHdrH=0
RowAlignHdrV=0
TabAlign=0
TabBW=1
TabCP=1
TabCS=1
TabCols=2
TabRows=2
TabWidth=100
TabWidthP=0
Template=
TemplateTag=
Title=
UseTemplate=0
CapStyleDefault=1
CapStyleBold=0
CapStyleFixedWidth=0
CapStyleItalic=0
CapStyleUnderline=0
RowStyleHdrDefault=1
RowStyleHdrBold=0
RowStyleHdrFixedWidth=0
RowStyleHdrItalic=0
RowStyleHdrUnderline=0
RowStyleGenDefault=1
RowStyleGenBold=0
RowStyleGenFixedWidth=0
RowStyleGenItalic=0
RowStyleGenUnderline=0
EnableBorderWidth=0
EnableCellPadding=0
EnableCellSpacing=0
EnableNumberOfColumns=0
EnableNumberOfRows=0
EnableTableWidth=0
IncludeColumnNames=0
[Properties]
AutoClose=0
AutoRun=0
Check4Untrans=0
Convert65535=0
Notify=1
SQLStmt=0
ShowWarnings=1
UseCompression=1
UserID={userID}
UserOption=3
DisplayLongSchemaNames=1
DisplayLongTableNames=0
DisplayLongColumnNames=1";

try
{
  File.WriteAllText($"{item}.dtfx", dtfxTemplate);
  Console.WriteLine($"Successfully created {item}.dtfx");
}
catch (System.Exception e)
{
  System.Console.WriteLine($"An error occured: {e.ToString()}\nPlease correct and try again");
}

}
//Create batch file for processing.

//if password was provided, create login line
string fileTransferTemplate = userPass.Length == 0 ? "" : $"{acsPath} /plugin=logon /system={hostName} /userid={userID} " + (userPass.Equals("") ? "" : ("/password=" + userPass));

string fileTransferBatchFileContents = "";
foreach (string outfile in outputFileNames)
{
  fileTransferBatchFileContents +=
  $@"
  {fileTransferTemplate}
  {acsPath} /plugin=download /userid={userID} {outfile}.dtfx";
}
try
{
  File.WriteAllText("TCGSourceFileTransfer.bat", fileTransferBatchFileContents);
  Console.WriteLine($"Successfully created TCGSourceFileTransfer.bat");
}
catch (System.Exception e)
{
  System.Console.WriteLine($"An error occured: {e.ToString()}\nPlease correct and try again");
}

