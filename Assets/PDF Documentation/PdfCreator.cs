using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;

public static class PdfCreator
{
    [SerializeField]
    private class DataElement
    {
        public string name, description, type;
        public string objectType;
        public string prefix;
        public string braceField;
        public string postFix;
        public int page;
        public int yPos;
        public List<string> parameters;
        public List<DataElement> children;
        
        public DataElement(string _name = "", string _description = "", string _type = "", string _objectType = "", string _prefix = "", string _braceField = "", string _postFix = "", List<string> _parameters = null, List<DataElement> _children = null)
        {
            name = _name;
            description = _description;
            type = _type;
            objectType = _objectType;
            prefix = _prefix;
            braceField = _braceField;
            postFix = _postFix;
            parameters = _parameters;
            page = 0;
            yPos = 0;
            if (parameters == null)
                parameters = new List<string>();
            children = _children;
            if (children == null)
                children = new List<DataElement>();
        }

        public DataElement GetCopy()
        {
            DataElement de = new DataElement(name, description, type, objectType, prefix, braceField, postFix, parameters, children);
            return de;
        }
    }

    private static Thread creatorThread;
    private static int objectCount;
    private const int pageHeight = 891;
    private const int pageWidth = 630;
    private static string[] modifiers = new string[18] { "abstract", "partial", "delegate", "async", "const", "event", "extern", "override", "readonly", "sealed", "static", "unsafe", "virtual", "volatile", "public", "private", "internal", "protected" };
    private static List<int> pageBreaks;
    private static List<int> tocPageBreaks;
    private static List<int> introPageBreaks;
    private static List<int> pageLengths;
    private static List<int> tocPageLengths;
    private static int firstPage;
    private static int lastPage;
    private static int titleIndex;
    private static int titleEnd;
    private static int introCount;
    private static int filesParsed = 0;
    public static int FilesParsed
    {
        get
        {
            return filesParsed;
        }
    }
    private static int totalFiles = 0;
    public static int TotalFiles
    {
        get
        {
            return totalFiles;
        }
    }

    //editor properties
    public static string ProjectRoot
    {
        get { return PlayerPrefs.GetString("PDFCreatorProjectRoot"); }
        set { PlayerPrefs.SetString("PDFCreatorProjectRoot", value); }
    }
    public static string PDFName
    {
        get { return PlayerPrefs.GetString("PDFCreatorPDFName"); }
        set { PlayerPrefs.SetString("PDFCreatorPDFName", value); }
    }
    public static string DocumentTitle
    {
        get { return PlayerPrefs.GetString("PDFCreatorDocumentTitle"); }
        set { PlayerPrefs.SetString("PDFCreatorDocumentTitle", value); }
    }
    public static bool PDFToRoot
    {
        get { return PlayerPrefs.GetInt("PDFCreatorPDFToRoot") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorPDFToRoot", value ? 1 : 0); }
    }
    public static string PDFFolder
    {
        get { return PlayerPrefs.GetString("PDFCreatorPDFFolder"); }
        set { PlayerPrefs.SetString("PDFCreatorPDFFolder", value); }
    }
    public static string IgnoreFiles
    {
        get { return PlayerPrefs.GetString("PDFCreatorIgnoreFiles"); }
        set { PlayerPrefs.SetString("PDFCreatorIgnoreFiles", value); }
    }
    public static string IgnoreFolders
    {
        get { return PlayerPrefs.GetString("PDFCreatorIgnoreFolders"); }
        set { PlayerPrefs.SetString("PDFCreatorIgnoreFolders", value); }
    }
    public static string IntroductionText
    {
        get { return PlayerPrefs.GetString("PDFCreatorIntroductionText"); }
        set { PlayerPrefs.SetString("PDFCreatorIntroductionText", value); }
    }
    public static bool IncludePrivates
    {
        get { return PlayerPrefs.GetInt("PDFCreatorIncludePrivates") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorIncludePrivates", value ? 1 : 0); }
    }
    public static bool IncludeEmpty
    {
        get { return PlayerPrefs.GetInt("PDFCreatorIncludeEmpty") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorIncludeEmpty", value ? 1 : 0); }
    }
    public static bool IncludeShaders
    {
        get { return PlayerPrefs.GetInt("PDFCreatorIncludeShaders") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorIncludeShaders", value ? 1 : 0); }
    }
    public static bool ShaderDescription
    {
        get { return PlayerPrefs.GetInt("PDFCreatorShaderDescription") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorShaderDescription", value ? 1 : 0); }
    }
    public static bool TocIncludeRootFolder
    {
        get { return PlayerPrefs.GetInt("PDFCreatorTocIncludeRootFolder") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeRootFolder", value ? 1 : 0); }
    }
    public static bool TocIncludeFolders
    {
        get { return PlayerPrefs.GetInt("PDFCreatorTocIncludeFolders") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeFolders", value ? 1 : 0); }
    }
    public static bool TocIncludeFiles
    {
        get { return PlayerPrefs.GetInt("PDFCreatorTocIncludeFiles") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeFiles", value ? 1 : 0); }
    }
    public static bool TocIncludeNamespaces
    {
        get { return PlayerPrefs.GetInt("PDFCreatorTocIncludeNamespaces") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeNamespaces", value ? 1 : 0); }
    }
    public static bool TocIncludeClasses
    {
        get { return PlayerPrefs.GetInt("PDFCreatorTocIncludeClasses") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeClasses", value ? 1 : 0); }
    }
    public static bool TocIncludeMethods
    {
        get { return PlayerPrefs.GetInt("PDFCreatorTocIncludeMethods") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeMethods", value ? 1 : 0); }
    }
    public static bool TocIncludeVariables
    {
        get{ return PlayerPrefs.GetInt("PDFCreatorTocIncludeVariables") == 1; }
        set { PlayerPrefs.SetInt("PDFCreatorTocIncludeVariables", value ? 1 : 0); }
    }
    public static string StatusText = "Ready";
    public static bool StatusTextGood = true;
    
    //private copies used only during document generation
    private static string projectRoot;
    private static string pDFName;
    private static string documentTitle;
    private static string pDFFolder;
    private static string ignoreFiles;
    private static string ignoreFolders;
    private static string introductionText;
    private static bool pDFToRoot;
    private static bool includePrivates;
    private static bool includeEmpty;
    private static bool includeShaders;
    private static bool shaderDescription;
    private static bool tocIncludeRootFolder;
    private static bool tocIncludeFolders;
    private static bool tocIncludeFiles;
    private static bool tocIncludeNamespaces;
    private static bool tocIncludeClasses;
    private static bool tocIncludeMethods;
    private static bool tocIncludeVariables;


    public static float progress
    {
        get
        {
            return (filesParsed * 1.0f) / (totalFiles * 1.0f);
        }
    }
    private static bool activeJob = false;
    public static bool ActiveJob
    {
        get
        {
            return activeJob;
        }
    }

    public static void InitEditorVariables()
    {
        if (!PlayerPrefs.HasKey("PDFCreatorProjectRoot"))
        {
            string initialValue = Application.dataPath;
            ProjectRoot = initialValue;
        }
        projectRoot = ProjectRoot;
        if (!PlayerPrefs.HasKey("PDFCreatorPDFName"))
        {
            string initialValue = "Documentation";  
            PDFName = initialValue;
        }
        if (PDFName.Length < 1)
        {
            PDFName = "Documentation";
        }
        pDFName = PDFName;
        if (!PlayerPrefs.HasKey("PDFCreatorDocumentTitle"))
        {
            string initialValue = Application.productName;
            DocumentTitle = initialValue;
        }
        documentTitle = DocumentTitle;
        if (!PlayerPrefs.HasKey("PDFCreatorPDFToRoot"))
        {
            bool initialValue = true;
            PDFToRoot = initialValue;
        }
        pDFToRoot = PDFToRoot;
        if (!PlayerPrefs.HasKey("PDFCreatorPDFFolder"))
        {
            string initialValue = Application.dataPath;
            PDFFolder = initialValue;
        }
        pDFFolder = PDFFolder;
        if (!PlayerPrefs.HasKey("PDFCreatorIgnoreFiles"))
        {
            string initialValue = "";
            IgnoreFiles = initialValue;
        }
        ignoreFiles = IgnoreFiles;
        if (!PlayerPrefs.HasKey("PDFCreatorIgnoreFolders"))
        {
            string initialValue = "";
            IgnoreFolders = initialValue;
        }
        ignoreFolders = IgnoreFolders;
        if (!PlayerPrefs.HasKey("PDFCreatorIntroductionText"))
        {
            string initialValue = "";
            IntroductionText = initialValue;
        }
        introductionText = IntroductionText;
        if (!PlayerPrefs.HasKey("PDFCreatorIncludePrivates"))
        {
            bool initialValue = true;
            IncludePrivates = initialValue;
        }
        includePrivates = IncludePrivates;
        if (!PlayerPrefs.HasKey("PDFCreatorIncludeEmpty"))
        {
            bool initialValue = true;
            IncludeEmpty = initialValue;
        }
        includeEmpty = IncludeEmpty;
        if (!PlayerPrefs.HasKey("PDFCreatorIncludeShaders"))
        {
            bool initialValue = true;
            IncludeShaders = initialValue;
        }
        includeShaders = IncludeShaders;
        if (!PlayerPrefs.HasKey("PDFCreatorShaderDescription"))
        {
            bool initialValue = true;
            ShaderDescription = initialValue;
        }
        shaderDescription = ShaderDescription;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeRootFolder"))
        {
            bool initialValue = true;
            TocIncludeRootFolder = initialValue;
        }
        tocIncludeRootFolder = TocIncludeRootFolder;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeFolders"))
        {
            bool initialValue = true;
            TocIncludeFolders = initialValue;
        }
        tocIncludeFolders = TocIncludeFolders;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeFiles"))
        {
            bool initialValue = true;
            TocIncludeFiles = initialValue;
        }
        tocIncludeFiles = TocIncludeFiles;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeNamespaces"))
        {
            bool initialValue = false;
            TocIncludeNamespaces = initialValue;
        }
        tocIncludeNamespaces = TocIncludeNamespaces;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeClasses"))
        {
            bool initialValue = false;
            TocIncludeClasses = initialValue;
        }
        tocIncludeClasses = TocIncludeClasses;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeMethods"))
        {
            bool initialValue = false;
            TocIncludeMethods = initialValue;
        }
        tocIncludeMethods = TocIncludeMethods;
        if (!PlayerPrefs.HasKey("PDFCreatorTocIncludeVariables"))
        {
            bool initialValue = false;
            TocIncludeVariables = initialValue;
        }
        tocIncludeVariables = TocIncludeVariables;

        StatusText = "Ready";
        StatusTextGood = true;
    }

    public static void ResetEditorValues()
    {
        ProjectRoot = Application.dataPath;
        PDFFolder = Application.dataPath;
        PDFName = "Documentation";
        DocumentTitle = Application.productName;
        IgnoreFiles = "";
        IgnoreFolders = "";
        IntroductionText = "";
        StatusText = "Ready";
        PDFToRoot = true;
        IncludePrivates = true;
        IncludeEmpty = true;
        IncludeShaders = true;
        ShaderDescription = true;
        TocIncludeRootFolder = true;
        TocIncludeFolders = true;
        TocIncludeFiles = true;
        TocIncludeNamespaces = false;
        TocIncludeClasses = false;
        TocIncludeMethods = false;
        TocIncludeVariables = false;
    }

    public static void CreateDocument()
    {
        if (!activeJob)
        {
            InitEditorVariables();
            string savePathBase = PDFToRoot ? ProjectRoot : PDFFolder;
            string path = savePathBase + "/" + pDFName + ".pdf";
            //replace the file with empty at first (or create it if it doesn't exist)
            bool cantRead = false;
            try
            {
                activeJob = true;
                File.WriteAllText(path, "");
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                cantRead = true;
                activeJob = false;
                Debug.LogWarning("Cannot write in " + path + ". Check if the given destination folder exists.\n\n" + e);
                StatusText = "Cannot write to file. Check if the given destination folder exists.";
                StatusTextGood = false;
            }
            catch (System.ArgumentException e)
            {
                cantRead = true;
                activeJob = false;
                Debug.LogWarning("Cannot write in " + path + ". Check if the file name contains illegal characters.\n\n" + e);
                StatusText = "Cannot write to file. Check if it contains illegal characters.";
                StatusTextGood = false;
            }
            catch (System.Exception e)
            {
                cantRead = true;
                activeJob = false;
                Debug.LogWarning("Cannot write in " + path + ". Check if the file is already open or if access is denied.\n\n" + e);
                StatusText = "Cannot write to file. Check if it is already open.";
                StatusTextGood = false;
            }

            if (!cantRead)
            {
                filesParsed = 0;
                totalFiles = 0;
                
                if (creatorThread != null && creatorThread.IsAlive)
                    Stop();

                activeJob = true;

                //find the image data file
                string imgPath = ImgPath();

                creatorThread = new Thread(new ParameterizedThreadStart(StartCreatingDocument));
                creatorThread.Start(imgPath);
                WaitForJobDone(creatorThread);
            }
        }
    }

    public static void Stop()
    {
        if (creatorThread != null)
        {
            if (creatorThread.IsAlive)
                creatorThread.Abort();
            creatorThread = null;
            activeJob = false;
        }
    }

    static void StartCreatingDocument(object obj)
    {
        string imgPath = obj.ToString();
        string savePathBase = pDFToRoot ? projectRoot : pDFFolder;
        string savePath = savePathBase + "/" + pDFName + ".pdf";

        introCount = 0;
        objectCount = 8;
        firstPage = 1;
        pageBreaks = new List<int>();
        pageLengths = new List<int>();
        tocPageBreaks = new List<int>();
        introPageBreaks = new List<int>();
        tocPageLengths = new List<int>();
        pageBreaks.Add(9);
        pageLengths.Add(850);
        tocPageLengths.Add(850);
        StatusText = "Counting files";
        StatusTextGood = true;

        string rootFolder = projectRoot; 
        try
        {
            int fileCount = FileCount(rootFolder);
            totalFiles = fileCount;

        }
        catch
        {
            activeJob = false;
            Debug.LogWarning("Cannot find project root: " + rootFolder + ". Check if the folder exists.");
            StatusText = "Couldn't find project root.";
            StatusTextGood = false;
            return;
        }

        StatusText = "Parsing files";
        StatusTextGood = true;

        DataElement rootData = ParseFolder(rootFolder);

        StatusText = "Creating document";
        StatusTextGood = true;
        try
        {
            string SRTitle = CreateText("Script Reference", 3, 30, 220, 830 - ExtraHeightFromPages());
            string dir = CreateData(rootData, 0, -80, out int height, false, true);


            //footers
            string footer = Footers(pageBreaks.Count, true);

            //main pages
            string mainPages = MainPages(pageBreaks.Count);

            //title and table of contents
            string titlePage = TitlePage();
            string introPages = IntroductionPage();
            string toc = TableOfContents(rootData);

            //add the amount of needed pages to object count
            int bonusPages = 3 + introPageBreaks.Count + tocPageBreaks.Count;
            if (introPages.Equals(""))
                bonusPages--;
            string bonusFooter = Footers(bonusPages-1, false);
            objectCount += bonusPages;

            string beginning = DocumentBeginning(pageBreaks.Count, bonusPages);
            string end = DocumentEnd(objectCount);

            bool cantRead = false;
            try
            {
                File.AppendAllText(savePath, beginning);
                File.AppendAllText(savePath, mainPages);
                File.AppendAllText(savePath, titlePage);
                File.AppendAllText(savePath, introPages);
                LoadImages(imgPath, savePath);
                File.AppendAllText(savePath, toc);
                File.AppendAllText(savePath, SRTitle);
                File.AppendAllText(savePath, dir);
                File.AppendAllText(savePath, footer);
                File.AppendAllText(savePath, bonusFooter);
                File.AppendAllText(savePath, end);
            }
            catch (System.Exception e)
            {
                cantRead = true;
                Debug.LogWarning("Cannot write in " + pDFName + ".pdf. Check if the file is already open or if access is denied.\n\n" + e);
                StatusText = "Cannot write in the file. Check if it is already open.";
                StatusTextGood = false;
            }

            if (!cantRead && imgPath.Length < 1)
            {
                Debug.LogWarning("Couldn't find image data. Make sure there 'imageData' file is under the PDF Documentation 'Editor' folder.");
                StatusText = "Couldn't find image data. PDF Documentation created to " + savePathBase;
                StatusTextGood = false;
            }

            if (!cantRead && imgPath.Length > 0)
            {
                Debug.Log("Successfully created PDF Documentation to " + savePathBase);
                StatusText = "PDF documentation created to " + savePathBase;
                StatusTextGood = true;
            }
        }
        catch (System.Exception e)
        {
            StatusText = "Error while creating document.";
            StatusTextGood = false; 
            Debug.LogWarning("Error while trying to create the PDF document:\n\n" + e);
        }


        activeJob = false;
    }

    private static string ImgPath()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath);

        FileInfo[] files = dir.GetFiles("imageData", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; ++i)
        {
            if (files[i].FullName.EndsWith("Editor\\imageData"))
            {
                return files[i].FullName;
            }
        }

        return "";
    }

    private static int FileCount(string f_path)
    {
        DirectoryInfo dir = new DirectoryInfo(f_path);
        int count = 0;

        string[] ignoredFiles = ignoreFiles.Split(',');
        for (int i = 0; i < ignoredFiles.Length; ++i)
        {
            ignoredFiles[i] = ignoredFiles[i].Trim();
        }

        FileInfo[] files = dir.GetFiles("*.cs", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Length; ++i)
        {
            bool ignore = false;
            for (int k = 0; k < ignoredFiles.Length; ++k)
            {
                if (ignoredFiles[k].Length > 0 && files[i].Name.Contains(ignoredFiles[k]))
                {
                    ignore = true;
                    break;
                }
            }

            if (ignore)
                continue;

            count++;
        }

        if (includeShaders)
        {
            FileInfo[] shaderFiles = dir.GetFiles("*.shader", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < shaderFiles.Length; ++i)
            {
                bool ignore = false;
                for (int k = 0; k < ignoredFiles.Length; ++k)
                {
                    if (ignoredFiles[k].Length > 0 && shaderFiles[i].Name.Contains(ignoredFiles[k]))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                    continue;

                count++;
            }

            FileInfo[] computeFiles = dir.GetFiles("*.compute", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < computeFiles.Length; ++i)
            {
                bool ignore = false;
                for (int k = 0; k < ignoredFiles.Length; ++k)
                {
                    if (ignoredFiles[k].Length > 0 && computeFiles[i].Name.Contains(ignoredFiles[k]))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                    continue;

                count++;
            }
        }

        string[] ignoredFolders = ignoreFolders.Split(',');
        for (int i = 0; i < ignoredFolders.Length; ++i)
        {
            ignoredFolders[i] = ignoredFolders[i].Trim();
        }

        //Go through all folders recursively
        DirectoryInfo[] folders = dir.GetDirectories();
        for (int i = 0; i < folders.Length; ++i)
        {
            bool ignore = false;
            for (int k = 0; k < ignoredFolders.Length; ++k)
            {
                if (ignoredFolders[k].Length > 0 && folders[i].Name.Contains(ignoredFolders[k]))
                {
                    ignore = true;
                    break;
                }
            }

            if (ignore)
                continue;

            count += FileCount(folders[i].FullName);
        }

        return count;
    }

    private static async void WaitForJobDone(Thread _t)
    {
        while (_t.IsAlive)
        {
            await Task.Yield();
        }
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private static DataElement ParseFolder(string rootFolder)
    {
        DirectoryInfo root = new DirectoryInfo(rootFolder);
        DataElement rootElement = new DataElement(root.Name, "", "Folder", "", "", "", "", null, new List<DataElement>());

        //Go through all files

        string[] ignoredFiles = ignoreFiles.Split(',');
        for (int i = 0; i < ignoredFiles.Length; ++i)
        {
            ignoredFiles[i] = ignoredFiles[i].Trim();
        }

        FileInfo[] files = root.GetFiles("*.cs", SearchOption.TopDirectoryOnly);
        
        for (int i = 0; i < files.Length; ++i)
        {
            bool ignore = false;
            for (int k = 0; k < ignoredFiles.Length; ++k)
            {
                //checking exact file names
                if (ignoredFiles[k].Length > 1 && ignoredFiles[k][0].Equals('"') && ignoredFiles[k][ignoredFiles[k].Length - 1].Equals('"'))
                {
                    if (files[i].Name.Contains(".") && (files[i].Name.Substring(0, files[i].Name.LastIndexOf('.')).Equals(ignoredFiles[k].Substring(1, ignoredFiles[k].Length - 2)) || files[i].Name.Equals(ignoredFiles[k].Substring(1, ignoredFiles[k].Length - 2))))
                    {
                        ignore = true;
                        break;
                    }
                }
                else if (ignoredFiles[k].Length > 0 && files[i].Name.Contains(ignoredFiles[k]))
                {
                    ignore = true;
                    break;
                }
            }

            if (ignore)
                continue;

            try
            {
                rootElement.children.Add(ParseCSFile(files[i].FullName));
            }
            catch (System.Exception e)
            {
                if (!e.GetType().Equals(typeof(ThreadAbortException)))
                    Debug.LogWarning("Couldn't parse file: " + files[i].FullName + ":\n" + e);
            }
            filesParsed++;
        }

        if (includeShaders)
        {
            FileInfo[] shaderFiles = root.GetFiles("*.shader", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < shaderFiles.Length; ++i)
            {
                bool ignore = false;
                for (int k = 0; k < ignoredFiles.Length; ++k)
                {
                    //checking exact file names
                    if (ignoredFiles[k].Length > 1 && ignoredFiles[k][0].Equals('"') && ignoredFiles[k][ignoredFiles[k].Length - 1].Equals('"'))
                    {
                        if (files[i].Name.Contains(".") && (files[i].Name.Substring(0, files[i].Name.LastIndexOf('.')).Equals(ignoredFiles[k].Substring(1, ignoredFiles[k].Length - 2)) || files[i].Name.Equals(ignoredFiles[k].Substring(1, ignoredFiles[k].Length))))
                        {
                            ignore = true;
                            break;
                        }
                    }
                    else if (ignoredFiles[k].Length > 0 && files[i].Name.Contains(ignoredFiles[k]))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                    continue;

                try
                {
                    rootElement.children.Add(ParseShader(shaderFiles[i].FullName));
                }
                catch (System.Exception e)
                {
                    if (!e.GetType().Equals(typeof(ThreadAbortException)))
                        Debug.LogWarning("Couldn't parse file: " + shaderFiles[i].FullName + ":\n" + e);
                }
                filesParsed++;
            }

            FileInfo[] computeFiles = root.GetFiles("*.compute", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < computeFiles.Length; ++i)
            {
                bool ignore = false;
                for (int k = 0; k < ignoredFiles.Length; ++k)
                {
                    if (ignoredFiles[k].Length > 0 && computeFiles[i].Name.Contains(ignoredFiles[k]))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                    continue;

                try
                {
                    rootElement.children.Add(ParseShader(computeFiles[i].FullName));
                }
                catch (System.Exception e)
                {
                    if (!e.GetType().Equals(typeof(ThreadAbortException)))
                        Debug.LogWarning("Couldn't parse file: " + computeFiles[i].FullName + ":\n" + e);
                }
                filesParsed++;
            }
        }

        string[] ignoredFolders = ignoreFolders.Split(',');
        for (int i = 0; i < ignoredFolders.Length; ++i)
        {
            ignoredFolders[i] = ignoredFolders[i].Trim();
        }

        //Go through all folders recursively
        DirectoryInfo[] folders = root.GetDirectories();
        for (int i = 0; i < folders.Length; ++i)
        {
            bool ignore = false;
            for (int k = 0; k < ignoredFolders.Length; ++k)
            {
                //checking exact folder names
                if (ignoredFolders[k].Length > 1 && ignoredFolders[k][0].Equals('"') && ignoredFolders[k][ignoredFolders[k].Length - 1].Equals('"'))
                {
                    if (folders[i].Name.Equals(ignoredFolders[k].Substring(1, ignoredFolders[k].Length - 2)))
                    {
                        ignore = true;
                        break;
                    }
                }
                else if (ignoredFolders[k].Length > 0 && folders[i].Name.Contains(ignoredFolders[k]))
                {
                    ignore = true;
                    break;
                }
            }

            if (ignore)
                continue;

            DataElement folder = ParseFolder(folders[i].FullName);

            if (folder.type == "Folder")
                rootElement.children.Add(folder);
        }

        //hide this folder if it has no relevant children
        if (rootElement.children.Count == 0)
            rootElement.type = "";

        return rootElement;
    }

    private static DataElement ParseCSFile(string path)
    {
        StreamReader reader = new StreamReader(path);
        string fullFile = reader.ReadToEnd();
        reader.Close();

        //remove comments
        fullFile = RemoveComments(fullFile);

        DataElement fileElement = new DataElement(new FileInfo(path).Name, "", "File", "", "", "", "", null, new List<DataElement>());

        //look for structures, delegates and namespaces
        int index = 0;
        while (index < fullFile.Length)
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            string nextWord = NextWord(fullFile, index);

            if (nextWord.Length < 1 || fullFile.IndexOf(nextWord, index) < index)
                break;

            if (nextWord.Equals("using"))
            {
                //skip until next ';'
                index = NextResult(fullFile, ';', index);
                continue;
            }

            if (nextWord.Contains("///") || nextWord.Equals("namespace") || nextWord.Equals("class") || nextWord.Equals("struct") || nextWord.Equals("interface") || nextWord.Equals("enum") || nextWord.Equals("delegate"))
            {
                DataElement classElement = ParseStructure(fullFile, index);

                if (classElement != null)
                {
                    fileElement.children.Add(classElement);
                }
                int altEnd = NextResult(fullFile, ';', index);
                int start = NextResult(fullFile, '{', index);
                if (altEnd >= 0 && (altEnd < start || start < 0))
                {
                    index = altEnd;
                }
                else
                {
                    int end = ClosingBrace(fullFile, '{', '}', start);
                    index = end;
                }
            }
            else
            {
                index = fullFile.IndexOf(nextWord, index) + nextWord.Length;
            }
        }

        return fileElement;
    }

    private static DataElement ParseStructure(string s, int ind)
    {
        if (ind < 0)
            ind = 0;
        
        DataElement element = new DataElement();
        string beginningWord = NextWord(s, ind);

        //checking if there's a summary comment
        if (beginningWord.Contains("///"))
        {
            element.description = ParseComment(s, s.IndexOf("///", ind));
            //removing the comment to avoid confusion with prefixes
            string next = NextWord(s, ind);
            while (next.Contains("///"))
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                int cstart = s.IndexOf("///", ind);
                int cend = s.IndexOf('\n', cstart);
                s = s.Remove(cstart, cend - cstart);
                next = NextWord(s, ind);
            }
        }

        string typeWord = NextWord(s, ind);


        while (!typeWord.Equals("class") && !typeWord.Equals("struct") && !typeWord.Equals("interface") && !typeWord.Equals("enum") && !typeWord.Equals("delegate") && !typeWord.Equals("namespace"))
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            ind = s.IndexOf(typeWord, ind) + typeWord.Length;
            typeWord = NextWord(s, ind);

            if (typeWord.Contains("///"))
            {
                int cstart = s.IndexOf("///", ind);
                int cend = s.IndexOf('\n', cstart);
                s = s.Remove(cstart, cend - cstart);
                typeWord = NextWord(s, ind);
            }
        }

        ind = s.IndexOf(typeWord, ind);
        
        if (typeWord == "enum")
        {
            element.type = "Enum";
            element.objectType = typeWord;
        }
        else if (typeWord == "namespace")
        {
            element.type = "Namespace";
            element.objectType = typeWord;
        }
        else if (typeWord == "delegate")
        {
            DataElement deleg = ParseVarFunc(s, Mathf.Max(ind, 0), element.name);
            deleg.description = element.description;
            return deleg;
        }
        else
        {
            element.type = "Class";
            element.objectType = typeWord;
        }

        //prefix ("accessibility")
        string prefix = "";
        int lastSemi = LastResult(s, ';', ind);
        int lastBrE = LastResult(s, '}', ind);
        int lastBrS = LastResult(s, '{', ind);
        int prefixStart = Mathf.Max(lastSemi, lastBrE, lastBrS, 0);
        prefix = s.Substring(prefixStart, Mathf.Max(ind, 0) - prefixStart).Trim();

        //prefix includes [...] fields
        while (prefix.Length > 0 && prefix[0].Equals('['))
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            int attrEnd = ClosingBrace(prefix, '[', ']', 0);
            if (attrEnd > 0)
            {
                element.braceField += prefix.Substring(0, attrEnd).Trim() + "\n";
                prefix = prefix.Substring(attrEnd).Trim();
            }
            else
            {
                break;
            }
        }
        if (element.braceField.Length > 0)
            element.braceField = element.braceField.Substring(0, element.braceField.Length - 1);

        element.prefix = prefix;

        ind = s.IndexOf(typeWord, Mathf.Max(ind, 0)) + typeWord.Length;
        ind = s.IndexOf(NextWord(s, Mathf.Max(ind, 0)), Mathf.Max(ind, 0));

        //name
        string name = "";
        for (int i = Mathf.Max(ind, 0); i < s.Length; ++i)
        {
            if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]) || s[i] == ':' || s[i] == '{' || s[i] == ';' || s[i] == '/' || i == s.Length - 1)
            {
                name = s.Substring(ind, i - ind).Trim();
                break;
            }
        }
        element.name = DropAllBrackets(name);

        ind = s.IndexOf(name, ind) + name.Length;

        //find the scope of the structure
        int dotdot = s.IndexOf(':', ind);
        int altEnd = s.IndexOf(';', ind);
        int start = s.IndexOf('{', ind);

        //inheritances
        if (dotdot >= 0 && (dotdot < altEnd || altEnd < 0) && (dotdot < start || start < 0))
        {
            int postfixEnd = altEnd < 0 || start < altEnd ? start : altEnd;
            string postfix = s.Substring(dotdot + 1, postfixEnd - dotdot - 1);
            element.postFix = " " + postfix.Trim();
        }

        if (altEnd >= 0 && altEnd < start)
            return element;

        int end = ClosingBrace(s, '{', '}', start);
        string sc = s.Substring(start + 1, end - start - 1);
        int sci = 0;

        string comment = "";
        //go through the scope
        if (element.type.Equals("Enum"))
        {
            //parse the enums like parameters
            sc = sc.Trim();
            if (sc.Length > 1)
            {
                List<string> enumList = new List<string>();
                int enumStart = 0;
                string enumComment = "";
                for (int i = 0; i < sc.Length; ++i)
                {
                    //skip comments
                    if (i < sc.Length - 2 && sc.Substring(i, 3).Equals("///"))
                    {
                        if (enumComment.Length == 0)
                        {
                            enumComment = ParseComment(sc, i);
                        }
                        sc = sc.Remove(i, sc.IndexOf('\n', i) - i);

                        --i;
                        continue;
                    }

                    if (sc[i].Equals(','))
                    {
                        enumList.Add(sc.Substring(enumStart, i - enumStart).Trim());
                        enumStart = i + 1;
                        if (enumComment.Length > 0)
                        {
                            element.description += "\n\n" + NextWord(enumList[enumList.Count - 1], 0).Trim() + ": " + enumComment;
                            enumComment = "";
                        }
                    }
                }
                //last parameter
                if (sc.Length > 0)
                {
                    enumList.Add(sc.Substring(enumStart, sc.Length - enumStart - 1).Trim());
                    if (enumComment.Length > 0)
                    {
                        element.description += "\n\n" + NextWord(enumList[enumList.Count - 1], 0).Trim() + ": " + enumComment + "\n";
                        enumComment = "";
                    }
                }
                element.parameters = enumList;
            }
        }
        else
        {
            while (sci >= 0 && sci < sc.Length)
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                //children
                int nextSqPar = sc.IndexOf('[', sci);
                int nextEqual = sc.IndexOf('=', sci);
                int nextSemiC = sc.IndexOf(';', sci);
                int nextNPar = sc.IndexOf('(', sci);
                int nextBrace = sc.IndexOf('{', sci);
                int nextComm = sc.IndexOf("///", sci);
                int nextEnd = nextEqual < 0 || (nextEqual > nextSemiC && nextSemiC >= 0) ? nextSemiC : nextEqual;
                nextEnd = nextEnd < 0 || (nextEnd > nextNPar && nextNPar >= 0) ? nextNPar : nextEnd;
                nextEnd = nextEnd < 0 || (nextEnd > nextBrace && nextBrace >= 0) ? nextBrace : nextEnd;
                if (nextEnd >= 0)
                {
                    if (nextComm >= 0 && nextComm < nextEnd && (nextSqPar < 0 || nextComm < nextSqPar) && comment == "")
                    {
                        //the child has a comment
                        comment = ParseComment(sc, sc.IndexOf("///", sci));
                        //removing the comment to avoid confusion with prefixes
                        string nextw = NextWord(sc, sci);
                        while (nextw.Contains("///"))
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            int cstart = sc.IndexOf("///", sci);
                            int cend = sc.IndexOf('\n', cstart);
                            sc = sc.Remove(cstart, cend - cstart);
                            nextw = NextWord(sc, sci);
                        }
                        continue;
                    }

                    sc = TrimBrackets(sc, sci, nextEnd);

                    if (nextSqPar < 0 || nextEnd < nextSqPar || !NextWord(sc, sci)[0].Equals('['))
                    {
                        //no fancy parenthesis on the way
                        string nextWord = NextWord(sc, sci);
                        while (IsModifier(nextWord))
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            sci += sc.IndexOf(nextWord, sci) - sci + nextWord.Length + 1;
                            nextWord = NextWord(sc, sci);
                        }
                    }
                    else
                    {
                        //[...]-fields before the actual data
                        while (NextWord(sc, sci)[0].Equals('['))
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            nextSqPar = NextResult(sc, '[', sci) - 1;
                            sci = ClosingBrace(sc, '[', ']', nextSqPar);
                        }
                        continue;
                    }

                    //now NextWord should be the type of the variable/function/class
                    int jumpTo = sci;
                    string next = NextWord(sc, Mathf.Max(sci, 0));

                    if (next.StartsWith(";"))
                    {
                        sci = NextResult(sc, ';', sci);
                        continue;
                    }

                    if (next.Equals("class") || next.Equals("enum") || next.Equals("interface") || next.Equals("struct") || next.Equals("namespace"))
                    {
                        DataElement child = ParseStructure(sc, Mathf.Max(sci, 0));

                        if (child != null)
                        {
                            if (comment != "")
                            {
                                child.description = comment + child.description;
                                comment = "";
                            }

                            element.children.Add(child);
                        }


                        int braceStart = NextResult(sc, '{', sci) - 1;
                        jumpTo = ClosingBrace(sc, '{', '}', braceStart);
                    }
                    else
                    {   
                        DataElement child = ParseVarFunc(sc, Mathf.Max(sci, 0), element.name);
                        if (comment != "")
                        {
                            child.description = comment;
                            comment = "";
                        }
                        //skipping all using-statements
                        if (child.type.Equals("using"))
                        {
                            int semiColon = NextResult(sc, ';', sci);
                            jumpTo = semiColon;
                        }
                        else if (child.type.Equals("Variable"))
                        {
                            int comma = NextResult(sc, ',', sci, true);
                            int semiColon = NextResult(sc, ';', sci);
                            int braceStart = NextResult(sc, '{', sci);
                            int equal = NextResult(sc, '=', sci);
                            if (((braceStart < 0 || semiColon < braceStart) && semiColon >= 0) || ((braceStart < 0 || equal < braceStart) && equal >= 0))
                            {
                                jumpTo = semiColon;
                                if (comma >= 0 && comma < semiColon)
                                {
                                    comma = 0;
                                    //if there's multiple variables declared, separate them
                                    while (comma >= 0 && comma < child.name.Length)
                                    {
                                        if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                                        DataElement copy = child.GetCopy();
                                        //find next comma, excluding braces
                                        int nextComma = NextResult(child.name, ',', comma, true);
                                        if (nextComma >= 0 && nextComma < child.name.Length)
                                            copy.name = child.name.Substring(comma, nextComma - comma-1).Trim();
                                        else
                                        {
                                            copy.name = child.name.Substring(comma, copy.name.Length - comma).Trim();
                                        }
                                        element.children.Add(copy);
                                        comma = nextComma;
                                    }
                                }
                                else
                                {
                                    element.children.Add(child);
                                }
                            }
                            else
                            {
                                jumpTo = ClosingBrace(sc, '{', '}', braceStart);
                                element.children.Add(child);
                            }
                        }
                        else if (child.type.Equals("Method") || child.type.Equals("Operator"))
                        {
                            int braceStart = NextResult(sc, '{', NextResult(sc, ')', sci)) - 1;
                            int semiColon = NextResult(sc, ';', NextResult(sc, ')', sci));
                            if ((braceStart < 0 || semiColon < braceStart) && semiColon >= 0)
                            {
                                jumpTo = semiColon;
                            }
                            else
                            {
                                jumpTo = ClosingBrace(sc, '{', '}', braceStart);
                            }
                            if (child.prefix != "empty")
                                element.children.Add(child);
                        }
                        else if (child.type.Equals("Property"))
                        {
                            int braceStart = NextResult(sc, '{', sci) - 1;
                            jumpTo = ClosingBrace(sc, '{', '}', braceStart);
                            if (NextWord(sc, jumpTo).StartsWith("="))
                            {
                                jumpTo = NextResult(sc, ';', jumpTo);
                            }
                            element.children.Add(child);
                        }

                        else if (child.type.Equals("Constructor") || child.type.Equals("Destructor"))
                        {
                            jumpTo = ClosingBrace(sc, '{', '}', NextResult(sc, '{', NextResult(sc, ')', sci)) - 1);
                            element.children.Add(child);
                        }

                    }

                    sci = jumpTo;
                }
                else
                    break;
            }
        }

        return element;
    }

    private static DataElement ParseVarFunc(string s, int ind, string cn)
    {
        DataElement element = new DataElement();
        //objectType
        string objectTypeWord = NextWord(s, ind);
        int nameInd = ind;
        if (objectTypeWord.Equals("using"))
        {
            element.type = "using";
            return element;
        }
        if ((objectTypeWord.StartsWith("~" + cn) && (objectTypeWord.Length == cn.Length + 1 || objectTypeWord[cn.Length + 1].Equals('('))))
        {
            element.name = "~" + cn;
            element.type = "Destructor";

        }
        else if (objectTypeWord.StartsWith(cn) && (NextWord(s, s.IndexOf(objectTypeWord, ind) + objectTypeWord.Length).StartsWith("(") || (objectTypeWord.Length > cn.Length && objectTypeWord[cn.Length].Equals('('))))
        {
            element.name = cn;
            element.type = "Constructor";
        }
        else
        {
            element.objectType = objectTypeWord;

            //name
            nameInd = s.IndexOf(objectTypeWord, ind) + objectTypeWord.Length + 1;
            int nameEnd = s.IndexOfAny(new char[4] { '(', '=', ';', '{' }, nameInd);
            string nameWord = s.Substring(nameInd, nameEnd - nameInd).Trim();
            element.name = nameWord;

            if (element.name.StartsWith("operator") && (element.name.Length == 8 || char.IsWhiteSpace(element.name[8]) || char.IsSeparator(element.name[8]) || "+-!~*/%&|^<>=".Contains(element.name[8].ToString())))
            {
                //operator
                element.type = "Operator";
                string operatorName = s.Substring(nameInd, NextResult(s, '(', nameInd) - nameInd - 1).Trim();
                element.name = operatorName;
            }
            else
            {
                element.name = DropAllBrackets(element.name);
                //type
                int braInd = s.IndexOf('{', nameInd);
                int parInd = s.IndexOf('(', nameInd);
                int nextEqual = s.IndexOf('=', nameInd);
                int nextSemiC = s.IndexOf(';', nameInd);
                int nextEnd = nextEqual < 0 || (nextEqual > nextSemiC && nextSemiC >= 0) ? nextSemiC : nextEqual;

                //checking if this is a delegate
                if (parInd > 0 && nextEqual > 0 && nextEqual < parInd)
                {
                    int parEnd = ClosingBrace(s, '(', ')', parInd);
                    string followingWord = NextWord(s, parEnd);
                    if (followingWord.Trim().StartsWith("=>"))
                        element.type = "Method";
                }

                if (element.type.Length < 1 && nextEnd >= 0 && (nextEnd < parInd || parInd < 0) && (nextEnd < braInd || braInd < 0))
                {
                    //variable
                    element.type = "Variable";
                }
                else if (element.type.Length < 1 && parInd >= 0 && (parInd < braInd || braInd < 0))
                {
                    //method
                    element.type = "Method";

                    int scopeStart = braInd + 1;
                    int scopeEnd = ClosingBrace(s, '{', '}', braInd) - 1;
                    if (nextSemiC > scopeStart || nextSemiC < 0)
                    {
                        //not an abstract method

                        if (scopeEnd < scopeStart)
                            throw new System.Exception("Couldn't find end of method " + element.name);
                        string scope = s.Substring(scopeStart, scopeEnd - scopeStart).Trim();
                        if (scope.Length == 0 && !includeEmpty)
                        {
                            element.prefix = "empty";
                            return element;
                        }
                    }

                }
                else if (element.type.Length < 1)
                {
                    //Property
                    element.type = "Property";

                    //gets and sets
                    int scopeStart = braInd + 1;
                    int scopeEnd = ClosingBrace(s, '{', '}', braInd) - 1;
                    if (scopeEnd < scopeStart)
                        throw new System.Exception("Couldn't find end of property " + element.name);
                    string scope = s.Substring(scopeStart, scopeEnd - scopeStart);


                    //finding get or set
                    int getInd = scope.IndexOf("get");
                    int setInd = scope.IndexOf("set");
                    if ((getInd < setInd || setInd < 0) && getInd >= 0)
                    {
                        element.parameters.Add("{ " + scope.Substring(0, getInd+3).Trim() + ";");
                        int getBrace = scope.IndexOf('{', scope.IndexOf("get"));
                        int getSemi = scope.IndexOf(';', scope.IndexOf("get"));
                        string afterGet = "";
                        if (getSemi >= 0 && (getBrace < 0 || getSemi < getBrace))
                        {
                            afterGet = scope.Substring(getSemi + 1);
                        }
                        else
                        {
                            afterGet = scope.Substring(ClosingBrace(scope, '{', '}', getBrace));
                        }

                        setInd = afterGet.IndexOf("set");
                        if (setInd < 0)
                        {
                            element.parameters.Add(" }");
                        }
                        else
                        {
                            element.parameters.Add(" " + afterGet.Substring(0, setInd + 3).Trim() + "; }");
                        }

                    }
                    else if ((setInd < getInd || getInd < 0) && setInd >= 0)
                    {
                        element.parameters.Add("{ " + scope.Substring(0, setInd + 3).Trim() + ";");
                        int getBrace = scope.IndexOf('{', scope.IndexOf("set"));
                        int getSemi = scope.IndexOf(';', scope.IndexOf("set"));
                        string afterSet = "";
                        if (getSemi >= 0 && (getBrace < 0 || getSemi < getBrace))
                        {
                            afterSet = scope.Substring(getSemi + 1);
                        }
                        else
                        {
                            afterSet = scope.Substring(ClosingBrace(scope, '{', '}', getBrace));
                        }

                        getInd = afterSet.IndexOf("get");
                        if (getInd < 0)
                        {
                            element.parameters.Add(" }");
                        }
                        else
                        {
                            element.parameters.Add(" " + afterSet.Substring(0, getInd + 3).Trim() + "; }");
                        }

                    }

                }
            }

        }

        if (element.type.Equals("Method") || element.type.Equals("Operator") || element.type.Equals("Constructor") || element.type.Equals("Destructor"))
        {

            //parameters
            int parInd = s.IndexOf('(', nameInd);
            string parameters = s.Substring(parInd + 1, ClosingBrace(s, '(', ')', parInd + 1) - parInd - 2);
            if (parameters.Length > 1)
            {
                List<string> paramList = new List<string>();
                int paramStart = 0;
                for (int i = 0; i < parameters.Length; ++i)
                {
                    if (parameters[i].Equals('"'))
                    {
                        bool noEscapes = false;
                        if (i - 2 >= 0 && parameters[i - 1].Equals('@'))
                            noEscapes = true;
                        ++i;
                        while (!parameters[i].Equals('"'))
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            //whatever is escaped, we skip it
                            if (!noEscapes && parameters[i].Equals('\\'))
                            {
                                i += 2;
                            }
                            else
                                i++;
                        }
                        continue;
                    }
                    if (parameters[i].Equals("'"[0]))
                    {
                        bool noEscapes = false;
                        if (i - 2 >= 0 && parameters[i - 1].Equals('@'))
                            noEscapes = true;

                        ++i;
                        while (!parameters[i].Equals("'"[0]))
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            //whatever is escaped, we skip it
                            if (!noEscapes && parameters[i].Equals('\\'))
                            {
                                i += 2;
                            }
                            else
                                i++;
                        }
                        continue;
                    }
                    if (parameters[i].Equals('<'))
                    {
                        ++i;
                        int indents = 0;
                        while (!parameters[i].Equals('>') || indents > 0)
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            if (parameters[i].Equals('<'))
                                ++indents;
                            else if (parameters[i].Equals('>'))
                                --indents;
                            ++i;
                        }
                        continue;
                    }
                    if (parameters[i].Equals('['))
                    {
                        ++i;
                        int indents = 0;
                        while (!parameters[i].Equals(']') || indents > 0)
                        {
                            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                            if (parameters[i].Equals('['))
                                ++indents;
                            else if (parameters[i].Equals(']'))
                                --indents;
                            ++i;
                        }
                        continue;
                    }
                    if (parameters[i].Equals(','))
                    {
                        paramList.Add(parameters.Substring(paramStart, i - paramStart).Trim());
                        paramStart = i + 1;
                    }
                }
                //last parameter
                if (parameters.Length > 0)
                {
                    paramList.Add(parameters.Substring(paramStart, parameters.Length - paramStart).Trim());
                }
                element.parameters = paramList;
            }

            //postfix (base)
            int parStart = NextResult(s, '(', ind) - 1;
            int parEnd = ClosingBrace(s, '(', ')', parStart);
            int scopeStart = NextResult(s, '{', parEnd);
            int semiColon = NextResult(s, ';', parEnd);
            int equal = NextResult(s, '=', parEnd); //possible for lambda expression
            int defEnd = scopeStart >= 0 && (semiColon < 0 || scopeStart < semiColon) ? scopeStart : semiColon;
            defEnd = equal >= 0 && (defEnd < 0 || equal < defEnd) ? equal : defEnd;
            int postFixDot = NextResult(s, ':', parEnd);
            if (postFixDot >= 0 && (postFixDot < defEnd || defEnd < 0))
            {
                element.postFix = s.Substring(postFixDot - 1, defEnd - postFixDot).Trim();
            }
        }

        //prefix ("accessibility")
        string prefix = "";
        int lastSemi = LastResult(s, ';', ind);
        int lastBrE = LastResult(s, '}', ind);
        int lastBrS = LastResult(s, '{', ind);
        int prefixStart = Mathf.Max(lastSemi, lastBrE, lastBrS, 0);
        prefix = s.Substring(prefixStart, ind - prefixStart).Trim();

        //prefix includes [...] fields
        while (prefix.Length > 0 && prefix[0].Equals('['))
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            int attrEnd = ClosingBrace(prefix, '[', ']', 0);
            if (attrEnd > 0)
            {
                element.braceField += prefix.Substring(0, attrEnd).Trim() + "\n";
                prefix = prefix.Substring(attrEnd).Trim();
            }
            else
            {
                break;
            }
        }
        if (element.braceField.Length > 0)
            element.braceField = element.braceField.Substring(0, element.braceField.Length - 1);

        element.prefix = prefix;

        return element;
    }

    private static string ParseComment(string s, int ind)
    {
        string comment = "";
        string summary = "";
        string returns = "";
        //find the scope (until something else than a ///-comment)
        int i = ind;
        while (i < s.Length && i >= 0 && NextWord(s, i).StartsWith("///"))
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            i = s.IndexOf('\n', i) + 1;
        }

        if (i < 0)
            return comment;

        string sc = s.Substring(ind, i - ind);

        int nextTag = sc.IndexOf('<');
        while (nextTag >= 0 && nextTag < sc.Length - 1)
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            int tagEnd = sc.IndexOf('>', nextTag);
            int tagStart = sc.IndexOf('<', nextTag + 1);
            if (tagEnd < nextTag || (tagStart >= 0 && tagStart < tagEnd))
            {
                //broken tag, aborting
                comment = "";
                break;
            }

            string tagContent = sc.Substring(nextTag + 1, tagEnd - nextTag - 1).Trim();
            string tagName = NextWord(tagContent, 0).Trim();

            int quickClose = sc.IndexOf("/>", nextTag);

            string tagExtra = "";
            if (quickClose >= 0 && tagEnd > quickClose)
            {
                //single-tag attribute. useless on it's own
                nextTag = sc.IndexOf('<', tagEnd);
                continue;
            }

            int longClose = sc.IndexOf("</" + tagName, nextTag);

            if (longClose < 0)
            {
                //broken tag, couldn't find closing, aborting

                comment = "";
                break;
            }

            int closingTagEnd = sc.IndexOf('>', longClose);
            int nextTagStart = sc.IndexOf('<', longClose + 1);
            if (closingTagEnd < 0 || (nextTagStart >= 0 && nextTagStart < tagEnd))
            {
                //broken tag, aborting
                comment = "";
                break;
            }

            string endTagContent = sc.Substring(longClose + 1, closingTagEnd - longClose - 1).Trim();

            if (!endTagContent.Equals("/" + tagName))
            {
                //broken tag, aborting
                comment = "";
                break;
            }

            if (!tagName.Length.Equals(tagContent))
                tagExtra = tagContent.Substring(tagContent.IndexOf(tagName) + tagName.Length).Trim();


            int endingTag = sc.IndexOf('<', closingTagEnd);

            string content = sc.Substring(tagEnd + 1, longClose - 1 - tagEnd);

            //eliminating extra comment line beginnings
            int prevBreak = 0;
            while (content.Contains("///"))
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                int breakStart = content.IndexOf('\n');
                if (breakStart < 0)
                    break;
                int breakEnd = content.IndexOf("///", breakStart) + 3;
                if (breakEnd > breakStart && breakStart >= 0)
                {
                    string tmpContent = content;
                    content = "";
                    if (breakStart != 0)
                        content = tmpContent.Substring(0, breakStart) + " ";
                    if (breakEnd < tmpContent.Length - 1)
                        content += tmpContent.Substring(breakEnd);
                }
                else
                {
                    break;
                }
            }

            while (content.Contains("\n"))
                content = content.Replace('\n', ' ');

            while (content.Contains("  "))
                content = content.Replace("  ", " ");

            //eliminating child tags inside the content
            List<int> paraPlaces = new List<int>();
            while (content.Contains("<"))
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                int breakStart = content.IndexOf('<');
                int breakEnd = content.IndexOf(">", breakStart) + 1;
                if (breakEnd > breakStart && breakStart >= 0)
                {
                    prevBreak = breakStart;
                    string childTag = content.Substring(breakStart + 1, breakEnd - breakStart - 2).Trim();

                    if (childTag.Equals("/para") || childTag.Equals("para/") || childTag.Equals("para"))
                    {
                        if (!paraPlaces.Contains(breakStart))
                            paraPlaces.Add(breakStart);
                    }
                    if (childTag[childTag.Length - 1].Equals('/') && (NextWord(childTag, 0).Equals("see") || NextWord(childTag, 0).Equals("seealso")))
                    {
                        //seealso or see -tag
                        childTag = childTag.Substring(childTag.IndexOf(" "));
                        childTag = childTag.Substring(0, childTag.Length - 1);

                        string[][] attributes = ParseAttributes(childTag);
                        if (attributes != null && attributes.Length > 0 && attributes[0] != null)
                        {
                            for (int k = 0; k < attributes[0].Length; ++k)
                            {
                                if (attributes[0][k].Equals("cref"))
                                {
                                    string newContent = attributes[1][k];
                                    content = content.Insert(breakStart, newContent);
                                    breakStart += newContent.Length;
                                    breakEnd += newContent.Length;
                                    break;
                                }
                            }
                        }
                    }
                    content = content.Remove(breakStart, breakEnd - breakStart);
                }
                else
                {
                    break;
                }
            }

            int lastIndex = 0;
            int placed = 0;
            for (int k = 0; k < paraPlaces.Count; ++k)
            {
                if (content.Substring(lastIndex, paraPlaces[k] + placed * 2 - lastIndex).Trim().Length > 0)
                {
                    content = content.Insert(paraPlaces[k] + placed * 2, "\n\n");
                    lastIndex = paraPlaces[k] + placed * 2;
                    placed++;
                }
            }

            while (content.Contains("\n "))
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");
                content = content.Replace("\n ", "\n");
            }

            switch (tagName)
            {
                case "summary":
                    summary = content.Trim();
                    break;

                case "returns":
                    if (content.Trim().Length > 0)
                        returns = "Returns: " + content.Trim();
                    break;

                case "param":
                    //extra should contain the name
                    string[][] attributes = ParseAttributes(tagExtra);
                    if (attributes != null && attributes.Length > 0 && attributes[0] != null)
                    {
                        for (int k = 0; k < attributes[0].Length; ++k)
                        {
                            if (attributes[0][k].Equals("name"))
                            {
                                if (content.Trim().Length > 0)
                                    comment += attributes[1][k] + ": " + content.Trim() + "\n";
                                break;
                            }
                        }
                    }
                    break;
            }

            nextTag = nextTagStart;
        }

        if (comment.Length > 0)
            comment = comment.Substring(0, comment.Length - 1);

        if (summary != "" && comment != "")
            comment = summary + "\n\n" + comment;
        else
            comment = summary + comment;

        if (comment != "" && returns != "")
            comment = comment + "\n\n" + returns;
        else
            comment = comment + returns;

        return comment;
    }

    private static string TrimBrackets(string s, int start, int end)
    {
        int trimmed = 0;
        int sqrCount = 0;
        int jpCount = 0;
        for (int i = start; i <= end - trimmed; ++i)
        {
            if (s[i].Equals('<'))
                jpCount++;
            if (s[i].Equals('>'))
                jpCount--;
            if (s[i].Equals('['))
                sqrCount++;
            if (s[i].Equals(']'))
                sqrCount--;

            if (sqrCount > 0 || jpCount > 0)
            {
                if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]))
                {
                    s = s.Remove(i, 1);
                    --i;
                    ++trimmed;
                }
            }
        }

        return s;
    }

    private static string DropAllBrackets(string s)
    {
        for (int i = 0; i < s.Length; ++i)
        {
            if (s[i].Equals('('))
            {
                int len = ClosingBrace(s, '(', ')', i) - i;
                if (len < 0)
                    return s;
                s = s.Remove(i, len);
                --i;
            }

            if (s[i].Equals('<'))
            {
                int len = ClosingBrace(s, '<', '>', i) - i;
                if (len < 0)
                    return s;
                s = s.Remove(i, len);
                --i;
            }

            if (s[i].Equals('['))
            {
                int len = ClosingBrace(s, '[', ']', i) - i;
                if (len < 0)
                    return s;
                s = s.Remove(i, len);
                --i;
            }
        }

        return s;
    }

    private static string[][] ParseAttributes(string s)
    {
        string[][] attributes = new string[2][];
        List<string> names = new List<string>();
        List<string> values = new List<string>();

        int ind = 0;
        while (ind >= 0 && ind < s.Length)
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            int nameEnd = s.IndexOf('=', ind);

            if (nameEnd < 0 || nameEnd >= s.Length)
                return null;

            string name = s.Substring(ind, nameEnd - ind).Trim();
            int dquotInd = s.IndexOf('"', nameEnd);
            int squotInd = s.IndexOf("'", nameEnd);
            if (dquotInd < 0 && squotInd < 0)
                return null;

            int qStart = 0;
            int qEnd = 0;
            if ((squotInd < 0 || dquotInd < squotInd) && dquotInd >= 0)
            {
                qStart = dquotInd;
                qEnd = s.IndexOf('"', dquotInd + 1);
            }
            else
            {
                qStart = squotInd;
                qEnd = s.IndexOf("'", squotInd + 1);
            }

            if (qEnd < qStart)
                return null;

            string value = s.Substring(qStart + 1, qEnd - qStart - 1);

            names.Add(name);
            values.Add(value);
            ind = qEnd + 1;
        }

        attributes[0] = names.ToArray();
        attributes[1] = values.ToArray();

        return attributes;
    }

    private static bool IsModifier(string s)
    {
        for (int i = 0; i < modifiers.Length; ++i)
        {
            if (modifiers[i].Equals(s))
                return true;
        }
        return false;
    }

    private static string RemoveNextIfComment(string s, int ind)
    {
        string next = NextWord(s, ind);

        while (next.StartsWith("///"))
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            int cstart = s.IndexOf("///", ind);
            int cend = s.IndexOf('\n', cstart);
            s = s.Remove(cstart, cend - cstart);
            next = NextWord(s, ind);
        }

        return s;
    }

    private static int LastResult(string s, char b, int ind)
    {
        if (ind < 0 || !s.Substring(0, ind).Contains(b + ""))
            return -1;

        int end = ind;
        for (int i = end; i >= 0; --i)
        {
            if (s[i].Equals(b))
            {
                end = i;
                break;
            }

            //skip quotes
            if (i >= 0 && s[i].Equals('"'))
            {
                i--;
                while (i >= 0 && s.Substring(i, 2) != "//" && (!s[i].Equals('"') || (i > 0 && (s[i-1].Equals('\\')))))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    i--;
                }
                continue;
            }
            if (i >= 0 && s[i].Equals("'"[0]))
            {
                i--;
                while (i >= 0 && s.Substring(i,2) != "//" && (!s[i].Equals("'"[0]) || (i > 0 && (s[i - 1].Equals('\\')))))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    i--;
                }
                continue;
            }
        }

        if (end == ind && s[ind] != b)
            return -1;

        return end + 1;
    }

    private static int NextResult(string s, char b, int ind, bool sb = false)
    {
        if (!s.Substring(ind).Contains(b + ""))
            return -1;

        int end = ind;
        for (int i = end; i < s.Length; ++i)
        {
            if (s[i].Equals(b))
            {
                end = i;
                break;
            }

            //skip braces if asked
            if (sb)
            {
                if (i < s.Length && s[i].Equals('{'))
                {
                    i = ClosingBrace(s, '{', '}', i + 1);
                    continue;
                }

                if (i < s.Length && s[i].Equals('['))
                {
                    i = ClosingBrace(s, '[', ']', i + 1);
                    continue;
                }
            }

            //skip comments
            if (i < s.Length - 2 && s.Substring(i, 3).Equals("///"))
            {
                i = s.IndexOf('\n', i);
                continue;
            }

            //skip quotes
            if (i < s.Length && s[i].Equals('"'))
            {
                bool noEscapes = false;
                if (i - 2 >= 0 && s[i - 1].Equals('@'))
                    noEscapes = true;
                i++;
                while (i < s.Length && !s[i].Equals('"'))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    //whatever is escaped, we skip it
                    if (!noEscapes && s[i].Equals('\\'))
                    {
                        i += 2;
                    }
                    else
                        i++;
                }
                continue;
            }
            if (i < s.Length && s[i].Equals("'"[0]))
            {
                bool noEscapes = false;
                if (i - 2 >= 0 && s[i - 1].Equals('@'))
                    noEscapes = true;
                i++;
                while (i < s.Length && !s[i].Equals("'"[0]))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    //whatever is escaped, we skip it
                    if (!noEscapes && s[i].Equals('\\'))
                    {
                        i += 2;
                    }
                    else
                        i++;
                }
                continue;
            }
        }

        if (end == ind && s[ind] != b)
            return -1;

        return end + 1;
    }

    private static int ClosingBrace(string s, char b, char cb, int ind)
    {
        if (ind < -1)
            return -1;

        int end = ind + 1;
        int extraParenthesis = 0;
        for (int i = end; i < s.Length; ++i)
        {
            //skip comments
            if (i < s.Length - 2 && s.Substring(i, 3).Equals("///"))
            {
                i = s.IndexOf('\n', i);
            }

            //skip quotes
            if (s[i].Equals('"'))
            {
                bool noEscapes = false;
                if (i - 2 >= 0 && s[i - 1].Equals('@'))
                    noEscapes = true;
                i++;
                while (i < s.Length && !s[i].Equals('"'))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    //whatever is escaped, we skip it
                    if (!noEscapes && s[i].Equals('\\'))
                    {
                        i += 2;
                    }
                    else
                        i++;
                }
            }
            if (i < s.Length && s[i].Equals("'"[0]))
            {
                bool noEscapes = false;
                if (i - 2 >= 0 && s[i - 1].Equals('@'))
                    noEscapes = true;
                i++;
                while (i < s.Length && !s[i].Equals("'"[0]))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    //whatever is escaped, we skip it
                    if (!noEscapes && s[i].Equals('\\'))
                    {
                        i += 2;
                    }
                    else
                        i++;
                }
            }

            if (i < s.Length && s[i].Equals(b))
                extraParenthesis++;
            else if (i < s.Length && s[i].Equals(cb))
            {
                extraParenthesis--;
                if (extraParenthesis < 0)
                {
                    //found the end
                    end = i;
                    break;
                }
            }
        }

        return end + 1;
    }

    private static int PrevWordIndex(string s, int ind)
    {
        int wordStart = ind;
        int length = 0;
        for (int i = ind; i >= 0; --i)
        {
            if (length < 1 && (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i])))
                wordStart--;
            else if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]))
            {
                break;
            }
            else
            {
                length++;
                wordStart--;
            }
        }

        return wordStart;
    }

    private static string NextWord(string s, int ind)
    {
        int wordStart = ind;
        int length = 0;

        for (int i = ind; i < s.Length; ++i)
        {
            if (length < 1 && (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i])))
                wordStart++;
            else if (char.IsWhiteSpace(s[i]) || char.IsSeparator(s[i]))
            {
                break;
            }
            else
            {
                length++;
            }
        }


        return s.Substring(wordStart, length);
    }

    private static string RemoveComments(string s)
    {
        s = s.Replace("\r", " ");

        int index = 1;
        string c = "";
        while (index < s.Length)
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            c = s[index - 1] + "" + s[index];

            //skip everyting inside quotes
            if (s[index - 1].Equals('"'))
            {
                bool noEscapes = false;
                if (index - 2 >= 0 && s[index - 2].Equals('@'))
                    noEscapes = true;

                index++;
                char k = s[index - 1];
                while (!k.Equals('"'))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    //whatever is escaped, we skip it
                    if (!noEscapes && k.Equals('\\'))
                    {
                        index += 2;
                    }
                    else
                        index++;
                    k = s[index - 1];
                }
                index++;
                continue;
            }

            if (s[index - 1].Equals("'"[0]))
            {
                bool noEscapes = false;
                if (index - 2 >= 0 && s[index - 2].Equals('@'))
                    noEscapes = true;

                index++;
                char k = s[index - 1];
                while (!k.Equals("'"[0]))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    if (!noEscapes && k.Equals('\\'))
                    {
                        index += 2;
                    }
                    else
                        index++;
                    k = s[index - 1];
                }
                index++;
                continue;
            }

            //oneliners
            if (c.Equals("//"))
            {
                if (index < s.Length - 1 && s[index + 1].Equals('/') && (LastCharacterEndOfObject(s, index - 1) || CommentInsideEnum(s, index - 2)))
                {
                    //summary comment. skip unless last character was not start of the file or ';' or '}'
                    index = s.IndexOf('\n', index) + 1;
                    string next = NextWord(s, index - 1);
                    while (next.StartsWith("///"))
                    {
                        if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                        index = s.IndexOf('\n', index) + 1;
                        next = NextWord(s, index - 1);
                    }
                    continue;
                }
                else
                {
                    //remove line
                    int lineEnd = s.IndexOf('\n', index);
                    if (lineEnd >= index)
                    {
                        s = s.Remove(index - 1, lineEnd - index + 1);
                    }
                    else
                    {
                        s = s.Remove(index - 1);
                    }
                    continue;
                }
            }

            else if (c.Equals("/*")) //multiliners
            {
                //remove comment 
                int lineEnd = s.IndexOf("*/", index);
                if (lineEnd < index)
                {
                    throw new System.Exception("Couldn't find end of a /* comment.");
                }
                s = s.Remove(index - 1, lineEnd - index + 3);
                continue;
            }

            //also remove preprocessor directives
            if (c.StartsWith("\u0023"))
            {
                //remove line
                int lineEnd = s.IndexOf('\n', index);
                if (lineEnd >= index)
                {
                    s = s.Remove(index - 1, lineEnd - index + 1);
                }
                else
                {
                    s = s.Remove(index - 1);
                }
                continue;
            }

            ++index;
        }
        return s;
    }

    private static bool LastCharacterEndOfObject(string s, int ind)
    {
        ind--;
        while (ind >= 0 && !s[ind].Equals('}') && !s[ind].Equals(';') && !s[ind].Equals('{') && !s[ind].Equals(']'))
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            if (char.IsWhiteSpace(s[ind]) || char.IsSeparator(s[ind]))
            {
                ind--;
                continue;
            }
            return false;
        }

        // ] is a special case, only works with attributes, not arrays for example
        if (ind >= 0 && s[ind].Equals(']'))
        {
            int sqrStart = LastResult(s, '[', ind);
            if (sqrStart > 0)
            {
                return LastCharacterEndOfObject(s, sqrStart - 1);
            }
        }

        return true;
    }

    private static bool CommentInsideEnum(string s, int ind)
    {
        int origInd = ind;
        if (ind >= 0 && s.Length > ind)
        {
            while (ind >= 0 && !s[ind].Equals(',') && !s[ind].Equals('{'))
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                if (char.IsWhiteSpace(s[ind]) || char.IsSeparator(s[ind]))
                {
                    ind--;
                    continue;
                }
                return false;
            }
        }
        else
        {
            return false;
        }

        if (ind >= 0 && s[ind].Equals(','))
        {
            int lastBr = LastResult(s, '{', ind);

            int lastBrEnd = LastResult(s, '}', ind);
            if (lastBrEnd < lastBr && lastBr > 0)
                ind = lastBr-1;


        }

        if (ind >= 1 && s[ind].Equals('{'))
        {
            ind = PrevWordIndex(s, ind-1);
            if (ind >= 1)
            {
                ind = PrevWordIndex(s, ind - 1);
                if (NextWord(s, ind).Equals("enum"))
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private static DataElement ParseShader(string path)
    {
        StreamReader reader = new StreamReader(path);
        string s = reader.ReadToEnd();
        reader.Close();

        DataElement element = new DataElement();
        element.name = new FileInfo(path).Name;
        element.type = "Shader";

        if (shaderDescription)
        {
            //parsing comments in the start for the description
            if (s.Length > 0)
            {
                string nextWord = NextWord(s, 0);
                int lastIndex = 0;
                while (nextWord.StartsWith("//") || nextWord.StartsWith("/*"))
                {
                    if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                    if (nextWord.StartsWith("//"))
                    {
                        int commentStart = s.IndexOf(nextWord, lastIndex) + 2;
                        int commentEnd = s.IndexOf("\n", commentStart);
                        if (commentEnd < 0)
                            commentEnd = s.Length;
                        element.description += s.Substring(commentStart, commentEnd - commentStart);
                        if (commentEnd == s.Length)
                            break;
                        nextWord = NextWord(s, commentEnd);
                        lastIndex = commentEnd;
                    }
                    else
                    {
                        int commentStart = s.IndexOf(nextWord, lastIndex) + 2;
                        int commentEnd = s.IndexOf("*/", commentStart);
                        if (commentEnd < 0)
                            commentEnd = s.Length;
                        element.description += s.Substring(commentStart, commentEnd - commentStart);
                        if (commentEnd == s.Length)
                            break;
                        nextWord = NextWord(s, commentEnd);
                        lastIndex = commentEnd;
                    }
                }
            }
        }

        return element;
    }

    private static string CreateData(DataElement data, float indent, int posY, out int height, bool includeTitle = false, bool root = false)
    {
        if (data != null)
        {
            if (data.type == "Folder")
            {
                return CreateFolder(data, indent, posY, out height, root);
            }
            else if (data.type == "File" || data.type == "Shader")
            {
                return CreateFile(data, indent, posY, out height);
            }
            else if (data.type == "Class" || data.type == "Namespace")
            {
                return CreateClass(data, indent, posY, out height, includeTitle);
            }
            else if (data.type == "Enum")
            {
                return CreateEnum(data, indent, posY, out height, includeTitle);
            }
            else if (data.type == "Variable")
            {
                return CreateVariable(data, indent, posY, out height, includeTitle);
            }
            else if (data.type == "Property")
            {
                return CreateProperty(data, indent, posY, out height, includeTitle);
            }
            else if (data.type == "Method" || data.type == "Constructor" || data.type == "Destructor" || data.type == "Operator")
            {
                return CreateMethod(data, indent, posY, out height, includeTitle);
            }
        }

        height = 0;
        return "";
    }

    private static string CreateFolder(DataElement data, float indent, int posY, out int height, bool root)
    {
        if (indent > 10)
            indent = 10;

        if (!root || tocIncludeRootFolder)
        {
            int ownHeight = 35;
            if (posY - ownHeight < 20 - ExtraHeightFromPages())
            {
                pageBreaks.Add(objectCount + 1);
                pageLengths.Add(-ExtraHeightFromPages(1) - posY);
            }

            data.page = pageBreaks.Count;
            data.yPos = posY + ExtraHeightFromPages();

            int posX = (int)(indent * 30 + 20);
            //create text for the name
            float nameWidth = 0;
            string displayName = "";
            for (int i = 0; i < data.name.Length - 1; ++i)
            {
                nameWidth += CharacterWidth(data.name[i], 2, 20);
                if (nameWidth > 500 - indent * 30)
                {
                    displayName = data.name.Substring(0, i + 1) + "...";
                    break;
                }
            }
            if (displayName.Equals(""))
                displayName = data.name;
            string nameText = CreateText(displayName, 2, 20, posX + 25, posY, 2, 1);
            //create line under the name
            string line = CreateLine(posX, posY - 6, 1f, new Color(0f, 0f, 0f));
            //folder icon
            string icon = CreateImage(posX, posY, 1, 20, 16);

            //create the children
            string children = "";
            int childrenHeight = 0;
            if (data.children != null)
            {
                float newIndent = indent >= 6 ? indent + 0.5f : indent + 1f;

                foreach (DataElement child in data.children)
                {
                    if (child.type.Equals("Shader"))
                    {
                        int childHeight = 0;
                        children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight);
                        childrenHeight += childHeight;
                    }
                }

                foreach (DataElement child in data.children)
                {
                    if (child.type.Equals("File"))
                    {
                        int childHeight = 0;
                        children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight);
                        childrenHeight += childHeight;
                    }
                }

                foreach (DataElement child in data.children)
                {
                    if (child.type.Equals("Folder"))
                    {
                        int childHeight = 0;
                        children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight);
                        childrenHeight += childHeight;
                    }
                }
            }

            height = ownHeight + childrenHeight;
            return icon + nameText + line + children;
        }
        else
        {
            data.page = 1;
            data.yPos = posY + ExtraHeightFromPages();

            //create only the children. this is the root folder
            string children = "";
            int childrenHeight = 0;
            if (data.children != null)
            {
                float newIndent = indent >= 6 ? indent + 0.5f : indent + 1f;

                foreach (DataElement child in data.children)
                {
                    if (child.type.Equals("Shader"))
                    {
                        int childHeight = 0;
                        children += CreateData(child, newIndent, posY - childrenHeight, out childHeight);
                        childrenHeight += childHeight;
                    }
                }

                foreach (DataElement child in data.children)
                {
                    if (child.type.Equals("File"))
                    {
                        int childHeight = 0;
                        children += CreateData(child, newIndent, posY - childrenHeight, out childHeight);
                        childrenHeight += childHeight;
                    }
                }
                foreach (DataElement child in data.children)
                {
                    if (child.type.Equals("Folder"))
                    {
                        int childHeight = 0;
                        children += CreateData(child, newIndent, posY - childrenHeight, out childHeight);
                        childrenHeight += childHeight;
                    }
                }
            }

            height = childrenHeight;
            return children;
        }
    }

    private static string CreateFile(DataElement data, float indent, int posY, out int height)
    {
        if (indent > 10)
            indent = 10;

        //some necessary preprocessing for the description box and text
        string[] splitTexts = SplitText(data.description, 2, 10, (int)(520 - indent * 30));
        int descHeight = 0;
        if (splitTexts.Length > 0 && splitTexts[0].Length > 0) descHeight += splitTexts.Length * 12 + 10;

        int ownHeight = 35 + descHeight;
        if (posY - ownHeight < 20 - ExtraHeightFromPages())
        {
            pageBreaks.Add(objectCount + 1);
            pageLengths.Add(-ExtraHeightFromPages(1) - posY);
        }

        data.page = pageBreaks.Count;
        data.yPos = posY + ExtraHeightFromPages();

        int posX = (int)(indent * 30 + 20);
        //create text for the name
        float nameWidth = 0;
        string displayName = "";
        for (int i = 0; i < data.name.Length -1; ++i)
        {
            nameWidth += CharacterWidth(data.name[i], 2, 18);
            if (nameWidth > 500 - indent * 30)
            {
                displayName = data.name.Substring(0, i + 1) + "...";
                break;
            }
        }
        if (displayName.Equals(""))
            displayName = data.name;
        string nameText = CreateText(displayName, 2, 18, posX + 25, posY, 2, 1);
        //create line under the name
        string line = CreateLine(posX, posY - 6, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        //.shader/.cs icon
        string icon = CreateImage(posX, posY - 3, data.type.Equals("File")? 2 : 3, 20, 20);
        //description
        string descText = CreateMultipleLinesText(splitTexts, 2, 10, posX + 10, posY - 20, 0, 1);

        //create the children
        string children = "";
        int childrenHeight = 0;
        if (data.children != null)
        {
            float newIndent = indent >= 6 ? indent + 0.5f : indent + 1f;

            //Namespaces
            bool firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Namespace"))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Enumerations
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Enum") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Classes
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Class") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Delegates (methods)
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Method") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }
        }

        height = ownHeight + childrenHeight;
        return icon + nameText + line + descText + children;
    }

    private static string CreateClass(DataElement data, float indent, int posY, out int height, bool includeTitle = false)
    {
        if (indent > 10)
            indent = 10;

        //some necessary preprocessing for the description box and text
        string[] splitTexts = SplitText(data.description, 2, 10, (int)(520 - indent * 30));

        string[] splitBraces = SplitText(data.braceField, 1, 10, (int)(450 - indent * 25));
        
        int descHeight = 0;
        int braceHeight = 0;
        int titleHeight = 0;
        if (splitTexts.Length > 0 && splitTexts[0].Length > 0) descHeight += splitTexts.Length * 12 + 10;
        if (splitBraces.Length > 0 && splitBraces[0].Length > 0) braceHeight += splitBraces.Length * 12;
        if (includeTitle) titleHeight = 50;

        string typeString = data.prefix + " " + data.objectType + " " + data.name;
        string[] splitType = SplitText(typeString, 1, 10, (int)(450 - indent * 25));
        int typeHeight = 0;
        if (splitType.Length > 0 && splitType[0].Length > 0) typeHeight += splitType.Length * 12;

        int ownHeight = 60 + descHeight + braceHeight + typeHeight + titleHeight;
        if (posY - ownHeight < 20 - ExtraHeightFromPages())
        {
            pageBreaks.Add(objectCount + 1);
            pageLengths.Add(-ExtraHeightFromPages(1) - posY);
        }

        data.page = pageBreaks.Count;
        data.yPos = posY + ExtraHeightFromPages();

        int posX = (int)(indent * 30 + 20);

        //possible title
        string title = "";
        string titleLine = "";
        if (includeTitle)
        {
            string titleString = data.type.Equals("Class") ? "Classes" : "Namespaces";
            title = CreateText(titleString, 2, 16, posX, posY - 20, 2, 1);
            titleLine = CreateLine(posX, posY - 26, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        }

        //create text for the name
        string displayedName = data.name.Length > (int)(65 - indent * 3) ? data.name.Substring(0, (int)(65 - indent * 3)) + "..." : data.name;
        string nameText = CreateText(displayedName, 1, 16, posX, posY - titleHeight, 2, 1);
        //create line under the name
        string line = CreateLine(posX, posY - 6 - titleHeight, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        //create box behind the description
        Color boxFill = data.type.Equals("Class") ? new Color(0.8f, 0.6f, 0.6f) : new Color(0.8f, 0.8f, 0.8f);
        string box = CreateBox(posX, posY - 30 - titleHeight - typeHeight - braceHeight, (int)(550 - indent * 30), braceHeight + typeHeight + 15, 0.5f, new Color(0.5f, 0.5f, 0.5f), boxFill);

        //create description text
        string braceText = CreateMultipleLinesText(splitBraces, 1, 10, posX + 10, posY - 30 - titleHeight, 0, 1, new Color(0.3f, 0.3f, 0.3f));
        string type = CreateMultipleLinesText(splitType, 1, 10, posX + 10, posY - 30 - braceHeight - titleHeight, 0, 1);
        string descText = CreateMultipleLinesText(splitTexts, 2, 10, posX + 10, posY - 50 - typeHeight - titleHeight - braceHeight, 0, 1);

        //create the children
        string children = "";
        int childrenHeight = 0;
        if (data.children != null)
        {
            float newIndent = indent >= 6 ? indent + 0.5f : indent + 1f; 
            //Constructors
            bool firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Constructor"))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Destructors
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Destructor"))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Operators
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Operator") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Enumerations
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Enum") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Classes
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Class") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Variables
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Variable") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Properties
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Property") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }

            //Methods
            firstChild = true;
            foreach (DataElement child in data.children)
            {
                if (child.type.Equals("Method") && (includePrivates || child.prefix.Contains("public")))
                {
                    int childHeight = 0;
                    children += CreateData(child, newIndent, posY - ownHeight - childrenHeight, out childHeight, firstChild);
                    childrenHeight += childHeight;
                    firstChild = false;
                }
            }
        }

        height = ownHeight + childrenHeight;
        return title + titleLine + nameText + line + box + type + braceText + descText + children;
    }

    private static string CreateEnum(DataElement data, float indent, int posY, out int height, bool includeTitle = false)
    {
        if (indent > 10)
            indent = 10;

        //some necessary preprocessing for the description box and text
        string[] splitTexts = SplitText(data.description, 2, 10, (int)(520 - indent * 30));

        string[] splitParameters = SplitTexts(data.parameters.ToArray(), 1, 10, (int)(425 - indent * 25), true);

        int descHeight = 0;
        int braceHeight = 0;
        int paramHeight = 0;
        int titleHeight = 0;
        
        if (splitParameters.Length > 0 && splitParameters[0].Length > 0)
        {
            string[] shortSplitParameters = null;
            if (splitParameters.Length > 40)
            {
                shortSplitParameters = new string[41];
                for (int i = 0; i < 40; ++i)
                {
                    shortSplitParameters[i] = splitParameters[i];
                }
                shortSplitParameters[40] = "...";
            }
            if (shortSplitParameters != null)
                splitParameters = shortSplitParameters;
            paramHeight += splitParameters.Length * 12;
            splitParameters[splitParameters.Length - 1] += "}";
        }
        string typeString = data.prefix + " " + data.objectType + " " + data.name;
        if (data.postFix.Length > 0)
        {
            typeString += "\n    : " + data.postFix.Trim();
        }
        typeString += "{";
        if (paramHeight == 0)
            typeString += "}";
        string[] splitType = SplitText(typeString, 1, 10, (int)(450 - indent * 25));
        int typeHeight = 0;
        if (splitType.Length > 0 && splitType[0].Length > 0) typeHeight += splitType.Length * 12;

        string[] splitBraces = SplitText(data.braceField, 1, 10, (int)(450 - indent * 25));

        if (splitTexts.Length > 0 && splitTexts[0].Length > 0) descHeight += splitTexts.Length * 12 + 10;
        if (splitBraces.Length > 0 && splitBraces[0].Length > 0) braceHeight += splitBraces.Length * 12;
        if (includeTitle) titleHeight = 50;


        int ownHeight = 60 + descHeight + typeHeight + braceHeight + paramHeight + titleHeight;
        if (posY - ownHeight < 20 - ExtraHeightFromPages())
        {
            pageBreaks.Add(objectCount + 1);
            pageLengths.Add(-ExtraHeightFromPages(1) - posY);
        }

        data.page = pageBreaks.Count;
        data.yPos = posY + ExtraHeightFromPages();

        int posX = (int)(indent * 30 + 20);

        //possible title
        string title = "";
        string titleLine = "";
        if (includeTitle)
        {
            title = CreateText("Enumerations", 2, 16, posX, posY - 20, 2, 1);
            titleLine = CreateLine(posX, posY - 26, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        }

        //create text for the name
        string displayedName = data.name.Length > (int)(65 - indent * 3) ? data.name.Substring(0, (int)(65 - indent * 3)) + "..." : data.name;
        string nameText = CreateText(displayedName, 1, 16, posX, posY - titleHeight, 2, 1);
        //create line under the name
        string line = CreateLine(posX, posY - 6 - titleHeight, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        //create box behind the description
        string box = CreateBox(posX, posY - 30 - titleHeight - braceHeight - typeHeight - paramHeight, (int)(550 - indent * 30), braceHeight + typeHeight + paramHeight + 15, 0.5f, new Color(0.5f, 0.5f, 0.5f), new Color(0.8f, 0.8f, 0.6f));

        //create description text
        string braceText = CreateMultipleLinesText(splitBraces, 1, 10, posX + 10, posY - 30 - titleHeight, 0, 1, new Color(0.3f, 0.3f, 0.3f));
        string type = CreateMultipleLinesText(splitType, 1, 10, posX + 10, posY - 30 - braceHeight - titleHeight, 0, 1);
        string paramText = CreateMultipleLinesText(splitParameters, 1, 10, posX + 40, posY - 30 - typeHeight - braceHeight - titleHeight, 0, 1);
        string descText = CreateMultipleLinesText(splitTexts, 2, 10, posX + 10, posY - 60 - paramHeight - typeHeight - braceHeight - titleHeight, 0, 1);

        height = ownHeight;
        return title + titleLine + nameText + line + box + type + braceText + paramText + descText;
    }

    private static string CreateMethod(DataElement data, float indent, int posY, out int height, bool includeTitle = false)
    {
        if (indent > 10)
            indent = 10;

        //some necessary preprocessing for the description box and text
        string[] splitTexts = SplitText(data.description, 2, 10, (int)(520 - indent * 30));

        string[] splitBraces = SplitText(data.braceField, 1, 10, (int)(450 - indent * 25));

        string[] splitParameters = SplitTexts(data.parameters.ToArray(), 1, 10, (int)(425 - indent * 25), true);

        string[] splitPostFix = SplitText(data.postFix.Replace('\n', ' '), 1, 10, (int)(440 - indent * 25));

        int descHeight = 0;
        int braceHeight = 0;
        int paramHeight = 0;
        int titleHeight = 0;
        int postFixHeight = 0;
        if (splitTexts.Length > 0 && splitTexts[0].Length > 0) descHeight += splitTexts.Length * 12 + 10;
        if (splitBraces.Length > 0 && splitBraces[0].Length > 0) braceHeight += splitBraces.Length * 12;
        if (splitPostFix.Length > 0 && splitPostFix[0].Length > 0) postFixHeight += splitPostFix.Length * 12;
        if (includeTitle) titleHeight = 50;

        if (splitParameters.Length > 0 && splitParameters[0].Length > 0)
        {
            paramHeight += splitParameters.Length * 12;
            splitParameters[splitParameters.Length - 1] += ")";
        }
        string typeString = data.prefix + " " + data.objectType + " " + data.name + "(";
        if (paramHeight == 0)
            typeString += ")";
        string[] splitType = SplitText(typeString, 1, 10, (int)(450 - indent * 25));
        int typeHeight = 0;
        if (splitType[0].Length > 0) typeHeight += splitType.Length * 12;

        int ownHeight = 60 + descHeight + typeHeight + braceHeight + postFixHeight + paramHeight + titleHeight;
        if (posY - ownHeight < 20 - ExtraHeightFromPages())
        {
            pageBreaks.Add(objectCount + 1);
            pageLengths.Add(-ExtraHeightFromPages(1) - posY);
        }

        data.page = pageBreaks.Count;
        data.yPos = posY + ExtraHeightFromPages();

        int posX = (int)(indent * 30 + 20);

        //possible title
        string title = "";
        string titleLine = "";
        if (includeTitle)
        {
            title = CreateText(data.type + "s", 2, 16, posX, posY - 20, 2, 1);
            titleLine = CreateLine(posX, posY - 26, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        }

        //create text for the name
        string displayedName = data.name.Length > (int)(65 - indent * 3) ? data.name.Substring(0, (int)(65 - indent * 3)) + "..." : data.name;
        string nameText = CreateText(displayedName, 1, 13, posX, posY - titleHeight, 2, 1);
        //create line under the name
        string line = CreateLine(posX, posY - 6 - titleHeight, 0.7f, new Color(0.6f, 0.6f, 0.6f));
        //create box behind the description
        Color boxFill = data.type.Equals("Method") ? new Color(0.6f, 0.8f, 0.6f) : (data.type.Equals("Operator") ? new Color(1.0f, 0.8f, 0.6f) : new Color(0.8f, 0.6f, 0.8f));
        string box = CreateBox(posX, posY - 30 - typeHeight - braceHeight - postFixHeight - titleHeight - paramHeight, (int)(550 - indent * 30), braceHeight + postFixHeight + paramHeight + typeHeight + 15, 0.5f, new Color(0.5f, 0.5f, 0.5f), boxFill);

        string braceText = CreateMultipleLinesText(splitBraces, 1, 10, posX + 10, posY - 30 - titleHeight, 0, 1, new Color(0.3f, 0.3f, 0.3f));
        string type = CreateMultipleLinesText(splitType, 1, 10, posX + 10, posY - 30 - braceHeight - titleHeight, 0, 1);
        string paramText = CreateMultipleLinesText(splitParameters, 1, 10, posX + 40, posY - 30 - typeHeight - braceHeight - titleHeight, 0, 1);
        string postFix = CreateMultipleLinesText(splitPostFix, 1, 10, posX + 20, posY - 30 - typeHeight - paramHeight - braceHeight - titleHeight, 0, 1);
        string descText = CreateMultipleLinesText(splitTexts, 2, 10, posX + 10, posY - 50 - typeHeight - paramHeight - postFixHeight - braceHeight - titleHeight, 0, 1);

        height = ownHeight;
        return title + titleLine + nameText + line + box + type + paramText + postFix + braceText + descText;
    }

    private static string CreateProperty(DataElement data, float indent, int posY, out int height, bool includeTitle = false)
    {
        if (indent > 10)
            indent = 10;

        //some necessary preprocessing for the description box and text
        string[] splitTexts = SplitText(data.description, 2, 10, (int)(520 - indent * 25));

        string[] splitBraces = SplitText(data.braceField, 1, 10, (int)(450 - indent * 25));

        string[] splitParameters = SplitTexts(data.parameters.ToArray(), 1, 10, (int)(425 - indent * 25), false);
        
        int descHeight = 0;
        int braceHeight = 0;
        int paramHeight = 0;
        int titleHeight = 0;
        if (splitTexts.Length > 0 && splitTexts[0].Length > 0) descHeight += splitTexts.Length * 12 + 10;
        if (splitBraces.Length > 0 && splitBraces[0].Length > 0) braceHeight += splitBraces.Length * 12;
        if (includeTitle) titleHeight = 50;
        if (splitParameters.Length > 0 && splitParameters[0].Length > 0)
        {
            paramHeight += splitParameters.Length * 12;
        }
        string typeString = data.prefix + " " + data.objectType + " " + data.name;
        if (paramHeight == 0)
            typeString += "}";
        string[] splitType = SplitText(typeString, 1, 10, (int)(450 - indent * 25));
        int typeHeight = 0;
        if (splitType.Length > 0 && splitType[0].Length > 0) typeHeight += splitType.Length * 12;

        int ownHeight = 60 + descHeight + typeHeight + braceHeight + paramHeight + titleHeight;
        if (posY - ownHeight < 20 - ExtraHeightFromPages())
        {
            pageBreaks.Add(objectCount + 1);
            pageLengths.Add(-ExtraHeightFromPages(1) - posY);
        }

        data.page = pageBreaks.Count;
        data.yPos = posY + ExtraHeightFromPages();

        int posX = (int)(indent * 30 + 20);

        //possible title
        string title = "";
        string titleLine = "";
        if (includeTitle)
        {
            title = CreateText("Properties", 2, 16, posX, posY - 20, 2, 1);
            titleLine = CreateLine(posX, posY - 26, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        }

        //create text for the name
        string displayedName = data.name.Length > (int)(65 - indent * 3) ? data.name.Substring(0, (int)(65 - indent * 3)) + "..." : data.name;
        string nameText = CreateText(displayedName, 1, 13, posX, posY - titleHeight, 2, 1);
        //create line under the name
        string line = CreateLine(posX, posY - 6 - titleHeight, 0.7f, new Color(0.6f, 0.6f, 0.6f));
        //create box behind the description
        string box = CreateBox(posX, posY - 30 - braceHeight - typeHeight - titleHeight - paramHeight, (int)(550 - indent * 30), braceHeight + paramHeight + typeHeight + 15, 0.5f, new Color(0.5f, 0.5f, 0.5f), new Color(0.6f, 0.8f, 0.8f));
        
        string braceText = CreateMultipleLinesText(splitBraces, 1, 10, posX + 10, posY - 30 - titleHeight, 0, 1, new Color(0.3f, 0.3f, 0.3f));
        string type = CreateMultipleLinesText(splitType, 1, 10, posX + 10, posY - 30 - braceHeight - titleHeight, 0, 1);
        string paramText = CreateMultipleLinesText(splitParameters, 1, 10, posX + 40, posY - 30 - braceHeight - typeHeight - titleHeight, 0, 1);
        string descText = CreateMultipleLinesText(splitTexts, 2, 10, posX + 10, posY - 50 - typeHeight - paramHeight - braceHeight - titleHeight, 0, 1);

        height = ownHeight;
        return title + titleLine + nameText + line + box + type + paramText + braceText + descText;
    }

    private static string CreateVariable(DataElement data, float indent, int posY, out int height, bool includeTitle = false)
    {
        if (indent > 10)
            indent = 10;

        //some necessary preprocessing for the description box and text
        string[] splitTexts = SplitText(data.description, 2, 10, (int)(520 - indent * 30));

        string[] splitBraces = SplitText(data.braceField, 1, 10, (int)(450 - indent * 25));

        string typeString = data.prefix + " " + data.objectType + " " + data.name;
        string[] splitType = SplitText(typeString, 1, 10, (int)(450 - indent * 25));

        int descHeight = 0;
        int braceHeight = 0;
        int titleHeight = 0;
        int typeHeight = 0;
        if (splitTexts.Length > 0 && splitTexts[0].Length > 0) descHeight += splitTexts.Length * 12 + 10;
        if (splitBraces.Length > 0 && splitBraces[0].Length > 0) braceHeight += splitBraces.Length * 12;
        if (splitType.Length > 0 && splitType[0].Length > 0) typeHeight += splitType.Length * 12;
        if (includeTitle) titleHeight = 50;

        int ownHeight = 60 + descHeight + braceHeight + typeHeight + titleHeight;
        if (posY - ownHeight < 20 - ExtraHeightFromPages())
        {
            pageBreaks.Add(objectCount + 1);
            pageLengths.Add(-ExtraHeightFromPages(1) - posY);
        }

        data.page = pageBreaks.Count;
        data.yPos = posY + ExtraHeightFromPages();

        int posX = (int)(indent * 30 + 20); 

        //possible title
        string title = "";
        string titleLine = "";
        if (includeTitle)
        {
            title = CreateText("Variables", 2, 16, posX, posY - 20, 2, 1);
            titleLine = CreateLine(posX, posY - 26, 0.85f, new Color(0.3f, 0.3f, 0.3f));
        }

        //create text for the name
        string displayedName = data.name.Length > (int)(65 - indent * 3) ? data.name.Substring(0, (int)(65 - indent * 3)) + "..." : data.name;
        string nameText = CreateText(displayedName, 1, 13, posX, posY - titleHeight, 2, 1);
        //create line under the name
        string line = CreateLine(posX, posY - 6 - titleHeight, 0.7f, new Color(0.6f, 0.6f, 0.6f));
        //create box behind the description
        string box = CreateBox(posX, posY - 30 - typeHeight - braceHeight - titleHeight, (int)(550 - indent * 30), braceHeight + typeHeight + 15, 0.5f, new Color(0.5f, 0.5f, 0.5f), new Color(0.6f, 0.6f, 0.8f));
        
        string braceText = CreateMultipleLinesText(splitBraces, 1, 10, posX + 10, posY - 30 - titleHeight, 0, 1, new Color(0.3f, 0.3f, 0.3f));
        string type = CreateMultipleLinesText(splitType, 1, 10, posX + 10, posY - 30 - braceHeight - titleHeight, 0, 1);
        string descText = CreateMultipleLinesText(splitTexts, 2, 10, posX + 10, posY - 50 - typeHeight - braceHeight - titleHeight, 0, 1);

        height = ownHeight;
        return title + titleLine + nameText + line + box + type + braceText + descText;
    }

    private static string CreateLine(int posX, int posY, float w = 1.0f, Color color = default)
    {
        objectCount++;
        posY += ExtraHeightFromPages();
        string streamContent = "\n" + color.r.ToString(CultureInfo.InvariantCulture) + " " + color.g.ToString(CultureInfo.InvariantCulture) + " " + color.b.ToString(CultureInfo.InvariantCulture) + " RG\n" + w.ToString(CultureInfo.InvariantCulture) + " w\n" + posX + " " + posY + " m\n1000 " + posY + " l S\n1 w\n0 0 0 RG\n";
        return objectCount + " 0 obj\n<</Length " + streamContent.Length + ">>\nstream" + streamContent + "endstream\nendobj\n";
    }

    private static string CreateBox(int startX, int startY, int width, int height, float w = 1.0f, Color border = default, Color fill = default)
    {
        objectCount++;
        startY += ExtraHeightFromPages();
        string streamContent = "\n" + fill.r.ToString(CultureInfo.InvariantCulture) + " " + fill.g.ToString(CultureInfo.InvariantCulture) + " " + fill.b.ToString(CultureInfo.InvariantCulture) + " rg\n" + border.r.ToString(CultureInfo.InvariantCulture) + " " + border.g.ToString(CultureInfo.InvariantCulture) + " " + border.b.ToString(CultureInfo.InvariantCulture) + " RG\n" + w.ToString(CultureInfo.InvariantCulture) + " w\n" + startX + " " + startY + " " + width + " " + height + " re B q\n0 0 0 rg\n0 0 0 RG\n1 w\n";
        return objectCount + " 0 obj\n<</Length " + streamContent.Length + ">>\nstream" + streamContent + "endstream\nendobj\n";
    }

    private static string CreateMultipleLinesText(string[] texts, int font, int fontSize, int posX, int posY, int render = 0, float width = 0, Color color = default, List<int> indentLines = null)
    {
        string result = "";
        string extraIndent = "    ";

        for (int i = 0; i < texts.Length; ++i)
        {
            if (indentLines != null && indentLines.Contains(i))
                texts[i] = extraIndent + texts[i];
            string line = CreateText(texts[i], font, fontSize, posX, posY - (fontSize + fontSize / 5) * i, render, width, color);
            result += line;
        }

        return result;
    }
    
    private static string CreateText(string text, int font, int fontSize, int posX, int posY, int render = 0, float width = 0, Color color = default)
    {
        objectCount++;
        posY += ExtraHeightFromPages();
        
        text = SanitizeString(text);
        string streamContent = "\nBT\n/F" + font + " " + fontSize + " Tf\n" + posX + " " + posY + " Td\n" + render + " Tr\n" + width.ToString(CultureInfo.InvariantCulture) + " w\n" + color.r.ToString(CultureInfo.InvariantCulture) + " " + color.g.ToString(CultureInfo.InvariantCulture) + " " + color.b.ToString(CultureInfo.InvariantCulture) + " RG\n" + color.r.ToString(CultureInfo.InvariantCulture) + " " + color.g.ToString(CultureInfo.InvariantCulture) + " " + color.b.ToString(CultureInfo.InvariantCulture) + " rg\n(" + text + ") Tj\n0 Tr\n1 w\n0 0 0 RG\n0 0 0 rg\nET\n";
        return objectCount + " 0 obj\n<</Length " + streamContent.Length + ">>\nstream" + streamContent + "endstream\nendobj\n";
    }

    //elements 1 -> 8: catalog, pages, fonts, images
    //elements 6 -> elementCount - pageCount * 2 - bonusPages: actual text etc. elements
    //elements elementCount - pageCount * 2 - bonusPages -> elementCount - pageCount - bonusPages: page numbers
    //elements elementCount - pageCount - bonusPages -> elementCount: page objects
    private static string DocumentBeginning(int pageCount, int bonusPages)
    {
        string catalog = "%PDF-1.7\n%This document was created using the 'PDF Documentation' Unity Asset\n%The asset can be found in https://assetstore.unity.com/packages/slug/179688\n1 0 obj\n<<\n/Type /Catalog\n%/OpenAction\n/Pages 2 0 R\n>>\nendobj\n";
        string pageContents = "";

        for (int i = objectCount - bonusPages + 1; i <= objectCount; ++i)
        {
            pageContents += i + " 0 R ";
        }
        for (int i = firstPage; i <= lastPage; ++i)
        {
            pageContents += i + " 0 R ";
        }
        string pages = "2 0 obj\n<<\n/Type /Pages\n/Count " + (pageCount + bonusPages) + "\n/Kids [" + pageContents + "]\n>>\nendobj\n";
        string fonts = "3 0 obj\n<<\n/Type /Font\n/Subtype /Type1\n/BaseFont /Courier-Oblique\n>>\nendobj\n4 0 obj\n<<\n/Type /Font\n/Subtype /Type1\n/BaseFont /Helvetica\n>>\nendobj\n5 0 obj\n<<\n/Type /Font\n/Subtype /Type1\n/BaseFont /Times-Roman\n>>\nendobj\n";
        
        //title
        string tPage = CreatePage(1, bonusPages, objectCount, titleIndex, titleEnd, false, false, false, true);
        
        string tocPages = "";
        //table of contents
        for (int i = 0; i < tocPageBreaks.Count + 1; ++i)
        {
            int startElement = i == 0 ? titleEnd + introCount : tocPageBreaks[i-1];
            int endElement = i == tocPageBreaks.Count ? objectCount - bonusPages * 2 + 2 : tocPageBreaks[i];
            string tocPage = CreatePage(i+2, bonusPages, objectCount, startElement, endElement, true, true, i == 0, true);
            tocPages += tocPage;
        }

        string introPages = "";
        //introduction text
        for (int i = tocPageBreaks.Count + 1; i < 2 + introPageBreaks.Count + tocPageBreaks.Count; ++i)
        {
            int startElement = i == tocPageBreaks.Count + 1 ? titleEnd : introPageBreaks[i - tocPageBreaks.Count - 2];
            int endElement = i == introPageBreaks.Count + tocPageBreaks.Count + 1 ? titleEnd + introCount : introPageBreaks[i - tocPageBreaks.Count - 1];
            string introPage = CreatePage(i + 2, bonusPages, objectCount, startElement, endElement, true, false, false, true);
            introPages += introPage;
        }

        return catalog + pages + tPage + introPages + tocPages + fonts;
    }

    private static string MainPages(int pageCount)
    {
        firstPage = objectCount + 1;
        objectCount += pageCount;
        lastPage = objectCount;

        string allPages = "";
        for (int i = 0; i < pageCount; ++i)
        {
            int endElement = i == pageCount - 1 ? objectCount - pageCount * 2 + 1 : pageBreaks[i + 1];
            string page = CreatePage(i + 1, pageCount, objectCount, pageBreaks[i], endElement, true);
            allPages += page;
        }

        return allPages;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageCount"></param>
    /// <param name="elementCount"></param>
    /// <param name="elementStart">Inclusive</param>
    /// <param name="elementEnd">Non-inclusive</param>
    /// <returns></returns>
    private static string CreatePage(int pageIndex, int pageCount, int elementCount, int elementStart, int elementEnd, bool includePageNumber, bool alternatingAnnotations = false, bool firstTitle = false, bool bonusPages = false)
    {
        string contents = "";
        string annots = "";
        if (!alternatingAnnotations)
        {
            for (int i = elementStart; i < elementEnd; ++i)
            {
                contents += i + " 0 R ";
            }
        }
        else
        {
            if (firstTitle)
            {
                contents += elementStart + " 0 R ";
                elementStart++;
            }
            for (int i = elementStart; i < elementEnd; i += 2)
            {
                contents += i + " 0 R ";
            }

            for (int i = elementStart+1; i < elementEnd; i += 2)
            {
                annots += i + " 0 R ";
            }
        }

        if (includePageNumber && !bonusPages)
            contents += (elementCount - pageCount * 2 + pageIndex) + " 0 R ";
        else if (includePageNumber && bonusPages)
            contents += (elementCount - pageCount * 2 + pageIndex) + " 0 R ";

        return (elementCount - pageCount + pageIndex) + " 0 obj\n<<\n/Type /Page\n/Parent 2 0 R\n/Annots [" + annots + "]\n/Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R >> /ProcSet [/PDF /ImageB/ImageC/ImageI] /XObject << /Im1 6 0 R /Im2 7 0 R /Im3 8 0 R >> >>\n/MediaBox [0 0 " + pageWidth + " " + pageHeight + "]\n/Contents [" + contents + "]\n>>\nendobj\n";
    }

    private static string CreateLink(int destPage, Rect pos, Vector3 dest)
    {
        objectCount++;
        return objectCount + " 0 obj\n<</Subtype/Link\n/Rect[" + pos.xMin + " " + pos.yMin + " " + pos.xMax + " " + pos.yMax + "]\n/BS <</W 0>>\n/F 4\n/Dest[" + destPage + " 0 R /XYZ " + dest.x + " " + dest.y + " " + dest.z + "]\n/StructParent 0>>\nendobj\n";
    }

    private static string TitlePage()
    {
        titleIndex = objectCount + 1;
        titleEnd = objectCount + 1;
        string pageText = "";
        int titleLines = 0;
        if (documentTitle != null && documentTitle.Length > 0)
        {
            string[] titleTexts = SplitText(documentTitle, 3, 40, 500);
            titleEnd += titleTexts.Length;
            float reverseIndent = 0;
            for (int i = 0; i < titleTexts[0].Length; ++i)
            {
                reverseIndent += CharacterWidth(titleTexts[0][i], 3, 40);
            }
            reverseIndent /= 1.83f;
            string titleText = CreateMultipleLinesText(titleTexts, 3, 40, pageWidth / 2 - (int)(reverseIndent), (int)(pageHeight/1.5f + titleTexts.Length * 20 - ExtraHeightFromPages()));
            pageText += titleText;
            titleLines += titleTexts.Length;
        }
        return pageText;
    }

    private static string IntroductionPage()
    {
        string titleText = "";
        string pageText = "";
        if (introductionText != null && introductionText.Length > 0)
        {
            //title
            titleText = CreateText("Introduction", 3, 30, 245, 830 - ExtraHeightFromPages());

            //main text
            string[] introTexts = SplitText(introductionText, 3, 15, 500);
            introCount = introTexts.Length+1;
            string introText = "";
            int firstSize = Mathf.Min(introTexts.Length, 39);
            string[] firstTexts = new string[firstSize];
            System.Array.Copy(introTexts, firstTexts, firstSize);
            introText = CreateMultipleLinesText(firstTexts, 3, 15, 50, 780 - ExtraHeightFromPages());
            for (int i = 39; i < introTexts.Length; i += 42)
            {
                introPageBreaks.Add(objectCount + 1);
                int batchSize = Mathf.Min(introTexts.Length - i, 42);
                string[] nextTexts = new string[batchSize];
                System.Array.Copy(introTexts, i, nextTexts, 0, batchSize);
                introText += CreateMultipleLinesText(nextTexts, 3, 15, 50, 835 - ExtraHeightFromPages());
            }
            pageText += introText;
        }

        return titleText + pageText;
    }

    private static string TableOfContents(DataElement root)
    {
        string toc = "";

        toc += TOCBeginning();

        int introObject = objectCount;
        int introLineY = -90 - ExtraHeightFromPages() + ExtraHeightFromPages(0, true);
        if (introCount > 0)
            objectCount += 2;

        int startY = introCount > 0 ? -110 : -90;
        string[] tocLines = TOCLine(root, 0, startY, true);
        for (int i = 0; i < tocLines.Length; ++i)
        {
            toc += tocLines[i];
        }

        string introContent = "";
        if (introCount > 0)
        {
            int totalObjectcount = objectCount;
            objectCount = introObject;
            string introLine = "Introduction............................................................................" + RomanNumeral(tocPageBreaks.Count+2);
            introContent += CreateText(introLine, 1, 10, 30, introLineY);
            introContent += CreateLink(totalObjectcount + 1 + 2*(2 + tocPageBreaks.Count), new Rect(30, 757, 72, 13), new Vector3(0, pageHeight, 0));
            objectCount = totalObjectcount;
        }
        toc += introContent;
        return toc;
    }

    private static string TOCBeginning()
    {
        //37 + N*40
        string title = CreateText("Table of Contents", 3, 30, 210, 830 - ExtraHeightFromPages());

        string lines = "";
        string titleLine = "Table of Contents.......................................................................I";
        lines += CreateText(titleLine, 1, 10, 30, -70 - ExtraHeightFromPages() + ExtraHeightFromPages(0, true));
        lines += CreateLink(1, new Rect(0, 0, 0, 0), new Vector3(0, 1, 0));


        return title + lines;
    }

    private static string[] TOCLine(DataElement data, int indent, int posY, bool root)
    {
        if (indent > 20)
            indent = 20;

        List<string> toc = new List<string>();

        if (root)
        {
            //Script reference title entry
            string scName = "Script Reference........................................................................";
            toc.Add(CreateText(scName + data.page, 1, 10, indent * 12 + 30, posY - ExtraHeightFromPages() + ExtraHeightFromPages(0, true)));
            toc.Add(CreateLink(data.page - 1 + firstPage, new Rect(indent * 12 + 30, posY + ExtraHeightFromPages(0, true) - 3, 16 * 6, 13), new Vector3(0, pageHeight, 0)));
            indent++;
            posY -= 20;
        }

        bool folder = data.type.Equals("Folder") && tocIncludeFolders;
        bool file = (data.type.Equals("File") || data.type.Equals("Shader")) && tocIncludeFiles;
        bool namespc = data.type.Equals("Namespace") && tocIncludeNamespaces;
        bool cls = (data.type.Equals("Class") || data.type.Equals("Enum")) && tocIncludeClasses;
        bool method = (data.type.Equals("Method") || data.type.Equals("Constructor") || data.type.Equals("Destructor") || data.type.Equals("Operator")) && tocIncludeMethods;
        bool variable = (data.type.Equals("Variable") || data.type.Equals("Property")) && tocIncludeVariables;
        bool included = folder || file || namespc || cls || method || variable;
        if ((tocIncludeRootFolder || !root) && included)
        {
            if (posY - 20 < 40 - ExtraHeightFromPages(0, true))
            {
                tocPageBreaks.Add(objectCount + 1);
                tocPageLengths.Add(-ExtraHeightFromPages(1, true) - posY);
            }

            string trueIndent = "";
            string OneIndent = "..";
            string trueName = "";
            for (int i = 0; i < 44 - indent; ++i)
            {
                trueIndent += OneIndent;
            }
            if (data.name.Length > trueIndent.Length - 3)
                trueName = data.name.Substring(0, trueIndent.Length - 3);
            else
                trueName = data.name;
            trueIndent = trueIndent.Substring(trueName.Length);
            toc.Add(CreateText(trueName + trueIndent + data.page, 1, 10, indent * 12 + 30, posY - ExtraHeightFromPages() + ExtraHeightFromPages(0, true)));
            toc.Add(CreateLink(data.page - 1 + firstPage, new Rect(indent * 12 + 30, posY + ExtraHeightFromPages(0, true) - 3, trueName.Length * 6, 13), new Vector3(indent * 30, data.yPos + 30, 0)));

        }
        else
        {
            indent -= 1;
        }

        string[] typeOrder = new string[12] { "Shader", "File", "Folder", "Namespace", "Constructor", "Destructor", "Operator", "Enum", "Class", "Variable", "Property", "Method" };

        //children
        for (int t = 0; t < typeOrder.Length; ++t)
        {
            for (int i = 0; i < data.children.Count; ++i)
            {
                if (data.children[i].type.Equals(typeOrder[t]))
                {
                    int nextY = root ? posY - 20 * (toc.Count / 2 - 1) : posY - 20 * (toc.Count / 2);
                    string[] child = TOCLine(data.children[i], indent + 1, nextY, false);
                    toc.AddRange(child);
                }

            }
        }

        return toc.ToArray();
    }

    private static string Footers(int pageCount, bool mainPages)
    {
        string pagenumbers = "";
        for (int i = 0; i < pageCount; ++i)
        {
            pagenumbers += PageNumber(i + 1, mainPages);
        }
        return pagenumbers;
    }

    private static string CreateImage(int posX, int posY, int ii, int width, int height)
    {
        objectCount++;
        return objectCount+" 0 obj\n<< /Length 56 >>\nstream\nq\n" + width + " 0 0 " + height + " " + posX + " " + (posY + ExtraHeightFromPages()) + " cm\n/Im" + ii + " Do\nQ\nendstream\nendobj\n";
    }

    private static bool LoadImages(string imageFolder, string endFile)
    {
        if (imageFolder.Length > 0)
        {
            return CopyBinaryFile(imageFolder, endFile);    
        }

        return false;
    }

    static bool CopyBinaryFile(string srcfilename, string destfilename)

    {
        if (File.Exists(srcfilename) == false)
        {
            StatusText = "Couldn't find the required image files";
            StatusTextGood = false;
            return false;
        }
        
        Stream s1 = File.Open(srcfilename, FileMode.Open);
        Stream s2 = File.Open(destfilename, FileMode.Append);
        
        BinaryReader f1 = new BinaryReader(s1);
        BinaryWriter f2 = new BinaryWriter(s2);
        
        while (true)
        {
            if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

            byte[] buf = new byte[10240];
            int sz = f1.Read(buf, 0, 10240);
            if (sz <= 0)
                break;
            f2.Write(buf, 0, sz);
            if (sz < 10240)
                break; // eof reached
        }
        f1.Close();
        f2.Close();
        return true;

    }

    private static string PageNumber(int pageNumber, bool number)
    {
        string pnText = "";
        if (number)
            pnText = CreateText("" + pageNumber, 3, 15, 40, 40 - ExtraHeightFromPages());
        else 
            pnText = CreateText("" + RomanNumeral(pageNumber), 3, 15, 40, 40 - ExtraHeightFromPages());

        return pnText;
    }

    private static string RomanNumeral(int number)
    {
        int[] decimals = new int[13] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        string[] romans = new string[13] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        
        string result = "";

        for (int index = 0; index < decimals.Length; index++)
        {
            while (decimals[index] <= number)
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                result += romans[index];
                number -= decimals[index];
            }
        }

        return result;
    }

    private static string DocumentEnd(int objectCount)
    {
        return "xref 0 " + (objectCount + 1) + "\ntrailer\n<<\n/Size " + (objectCount + 1) + "\n/Root 1 0 R\n>>\nstartxref\n%%EOF";
    }

    private static string[] SplitTexts(string[] s, int font, int fontSize, int c, bool commas)
    {
        List<string> splitTexts = new List<string>();
        for (int i = 0; i < s.Length; ++i)
        {
            s[i] = s[i].Replace('\n', ' ');
            while (s[i].Contains("  "))
            {
                if (!activeJob) throw new System.Exception("PDF generation stopped manually.");

                s[i] = s[i].Replace("  ", " ");
            }

            splitTexts.AddRange(SplitText(s[i].Replace('\n', ' '), font, fontSize, c));
            if (commas && i != s.Length-1)
                splitTexts[splitTexts.Count - 1] += ",";
        }

        return splitTexts.ToArray();
    }

    private static string[] SplitText(string s, int font, int fontSize, int c)
    {
        if (s.Length == 0)
            return new string[0];

        string[] splitTexts = s.Trim().Split('\n');
        
        List<string> tmpSplitTexts = new List<string>();
        for (int i = 0; i < splitTexts.Length; ++i)
        {
            string[] splitLine = SplitString(splitTexts[i], font, fontSize, c);
            for (int k = 0; k < splitLine.Length; ++k)
            {
                tmpSplitTexts.Add(splitLine[k].Trim());
            }
        }

        if (tmpSplitTexts.Count > 0)
            splitTexts = tmpSplitTexts.ToArray();


        return splitTexts;
    }

    private static string SanitizeString(string s)
    {
        s = s.Replace("\\", "\\\\");
        s = s.Replace("Æ", "\\306");
        s = s.Replace("Á", "\\301");
        s = s.Replace("Â", "\\302");
        s = s.Replace("Ä", "\\304");
        s = s.Replace("À", "\\300");
        s = s.Replace("Å", "\\305");
        s = s.Replace("Ã", "\\303");
        s = s.Replace("Ç", "\\307");
        s = s.Replace("É", "\\311");
        s = s.Replace("Ê", "\\312");
        s = s.Replace("Ë", "\\313");
        s = s.Replace("È", "\\310");
        s = s.Replace("Ð", "\\320");
        s = s.Replace("€", "\\240");
        s = s.Replace("Í", "\\315");
        s = s.Replace("Î", "\\316");
        s = s.Replace("Ï", "\\317");
        s = s.Replace("Ì", "\\314");
        s = s.Replace("Ł", "\\225");
        s = s.Replace("Ñ", "\\321");
        s = s.Replace("Œ", "\\226");
        s = s.Replace("Ó", "\\323");
        s = s.Replace("Ô", "\\324");
        s = s.Replace("Ö", "\\326");
        s = s.Replace("Ò", "\\322");
        s = s.Replace("Ø", "\\330");
        s = s.Replace("Õ", "\\325");
        s = s.Replace("Š", "\\227");
        s = s.Replace("Þ", "\\336");
        s = s.Replace("Ú", "\\332");
        s = s.Replace("Û", "\\333");
        s = s.Replace("Ü", "\\334");
        s = s.Replace("Ù", "\\331");
        s = s.Replace("Ý", "\\335");
        s = s.Replace("Ÿ", "\\230");
        s = s.Replace("Ž", "\\231");
        s = s.Replace("á", "\\341");
        s = s.Replace("â", "\\342");
        s = s.Replace("ä", "\\344");
        s = s.Replace("æ", "\\346");
        s = s.Replace("à", "\\340");
        s = s.Replace("å", "\\345");
        s = s.Replace("´", "\\264");
        s = s.Replace("~", "\\176");
        s = s.Replace("*", "\\052");
        s = s.Replace("@", "\\100");
        s = s.Replace("ã", "\\343");
        s = s.Replace("|", "\\174");
        s = s.Replace("˘", "\\030");
        s = s.Replace("¦", "\\246");
        s = s.Replace("•", "\\200");
        s = s.Replace("ˇ", "\\031");
        s = s.Replace("ç", "\\347");
        s = s.Replace("¸", "\\270");
        s = s.Replace("¢", "\\242");
        s = s.Replace("ˆ", "\\032");
        s = s.Replace("^", "\\136");
        s = s.Replace(":", "\\072");
        s = s.Replace(",", "\\054");
        s = s.Replace("©", "\\251");
        s = s.Replace("¤", "\\244");
        s = s.Replace("†", "\\201");
        s = s.Replace("‡", "\\202");
        s = s.Replace("°", "\\260");
        s = s.Replace("¨", "\\250");
        s = s.Replace("÷", "\\367");
        s = s.Replace("$", "\\044");
        s = s.Replace("ı", "\\232");
        s = s.Replace("é", "\\351");
        s = s.Replace("ê", "\\352");
        s = s.Replace("ë", "\\353");
        s = s.Replace("è", "\\350");
        s = s.Replace("—", "\\204");
        s = s.Replace("–", "\\205");
        s = s.Replace("ð", "\\360");
        s = s.Replace("¡", "\\241");
        s = s.Replace("ƒ", "\\206");
        s = s.Replace("⁄", "\\207");
        s = s.Replace("ß", "\\337");
        s = s.Replace("`", "\\140");
        s = s.Replace("«", "\\253");
        s = s.Replace("»", "\\273");
        s = s.Replace("‹", "\\210");
        s = s.Replace("›", "\\211");
        s = s.Replace("í", "\\355");
        s = s.Replace("î", "\\356");
        s = s.Replace("ï", "\\357");
        s = s.Replace("¬", "\\254");
        s = s.Replace("ł", "\\233");
        s = s.Replace("ˉ", "\\257");
        s = s.Replace("μ", "\\265");
        s = s.Replace("×", "\\327");
        s = s.Replace("ñ", "\\361");
        s = s.Replace("ó", "\\363");
        s = s.Replace("ô", "\\364");
        s = s.Replace("ö", "\\366");
        s = s.Replace("œ", "\\234");
        s = s.Replace("˛", "\\035");
        s = s.Replace("ò", "\\362");
        s = s.Replace("½", "\\275");
        s = s.Replace("¼", "\\274");
        s = s.Replace("¹", "\\271");
        s = s.Replace("ª", "\\252");
        s = s.Replace("º", "\\272");
        s = s.Replace("ø", "\\370");
        s = s.Replace("õ", "\\365");
        s = s.Replace("¶", "\\266");
        s = s.Replace("(", "\\050");
        s = s.Replace(")", "\\051");
        s = s.Replace("%", "\\045");
        s = s.Replace(".", "\\056");
        s = s.Replace("·", "\\267");
        s = s.Replace("‰", "\\213");
        s = s.Replace("±", "\\261");
        s = s.Replace("¿", "\\277");
        s = s.Replace("„", "\\214");
        s = s.Replace("“", "\\215");
        s = s.Replace("”", "\\216");
        s = s.Replace("‘", "\\217");
        s = s.Replace("’", "\\220");
        s = s.Replace("‚", "\\221");
        s = s.Replace("'", "\\047");
        s = s.Replace("®", "\\256");
        s = s.Replace("š", "\\235");
        s = s.Replace("§", "\\247");
        s = s.Replace("£", "\\243");
        s = s.Replace("þ", "\\376");
        s = s.Replace("¾", "\\276");
        s = s.Replace("³", "\\263");
        s = s.Replace("™", "\\222");
        s = s.Replace("²", "\\262");
        s = s.Replace("ú", "\\372");
        s = s.Replace("û", "\\373");
        s = s.Replace("ü", "\\374");
        s = s.Replace("ù", "\\371");
        s = s.Replace("ý", "\\375");
        s = s.Replace("ÿ", "\\377");
        s = s.Replace("¥", "\\245");
        s = s.Replace("ž", "\\236");
        return s;
    }

    private static int CutIndex(string s, int font, int fontSize, int cut)
    {
        float cumulativeWidth = 0;
        for (int i = 0; i < s.Length; ++i)
        {
            cumulativeWidth += CharacterWidth(s[i], font, fontSize);
            if (cumulativeWidth >= cut)
                return i;
        }
        
        return s.Length;
    }

    private static float CharacterWidth(char c, int font, float fontSize)
    {
        if (font == 2)
        {
            fontSize /= 1.9f;

            if ("\n\r".Contains(c + ""))
                return 0.01f;

            if ("abdeghnopquL?#1234567890".Contains(c + ""))
                return fontSize;

            if ("ijl\'.,".Contains(c + ""))
                return 0.4f * fontSize;

            if ("ftI/\\![] ".Contains(c + ""))
                return 0.5f * fontSize;

            if ("r-(){}".Contains(c + ""))
                return 0.6f * fontSize;

            if ("\"".Contains(c + ""))
                return 0.65f * fontSize;

            if ("cksvxyzJ".Contains(c + ""))
                return 0.9f * fontSize;

            if ("+=".Contains(c + ""))
                return 1.05f * fontSize;

            if ("FTZ".Contains(c + ""))
                return 1.1f * fontSize;

            if ("&".Contains(c + ""))
                return 1.18f * fontSize;

            if ("ABEKPSVXY".Contains(c + ""))
                return 1.2f * fontSize;

            if ("wCDHNRU".Contains(c + ""))
                return 1.3f * fontSize;

            if ("GOQ".Contains(c + ""))
                return 1.4f * fontSize;

            if ("mM".Contains(c + ""))
                return 1.5f * fontSize;

            if ("%".Contains(c + ""))
                return 1.6f * fontSize;

            if ("W".Contains(c + ""))
                return 1.7f * fontSize;

            if ("@".Contains(c + ""))
                return 1.8f * fontSize;
        }
        else if (font == 3)
        {
            fontSize /= 2.4f;

            if ("\n\r".Contains(c + ""))
                return 0.01f;

            if ("acez?".Contains(c + ""))
                return fontSize;

            if ("., ".Contains(c + ""))
                return 0.55f * fontSize;

            if ("'".Contains(c + ""))
                return 0.6f * fontSize;

            if ("ijlt/\\".Contains(c + ""))
                return 0.62f * fontSize;

            if ("frI!()´`-[]".Contains(c + ""))
                return 0.75f * fontSize;

            if ("s\"".Contains(c + ""))
                return 0.9f * fontSize;

            if ("J".Contains(c + ""))
                return 0.95f * fontSize;

            if ("^{}".Contains(c + ""))
                return 1.05f * fontSize;

            if ("g*$£1234567890".Contains(c + ""))
                return 1.1f * fontSize;

            if ("bdhknopquvxy#".Contains(c + ""))
                return 1.12f * fontSize;

            if ("FPS=+".Contains(c + ""))
                return 1.25f * fontSize;

            if ("ELTZ".Contains(c + ""))
                return 1.37f * fontSize;

            if ("BCR".Contains(c + ""))
                return 1.5f * fontSize;

            if ("ADGHKNOQUVXY".Contains(c + ""))
                return 1.6f * fontSize;

            if ("w".Contains(c + ""))
                return 1.65f * fontSize;

            if ("m&".Contains(c + ""))
                return 1.75f * fontSize;

            if ("%".Contains(c + ""))
                return 1.9f * fontSize;

            if ("M@W".Contains(c + ""))
                return 2f * fontSize;
        }
        else if (font == 1)
        {
            return fontSize / 2f;
        }

        return fontSize;
    }

    private static string[] SplitString(string s, int font, int fontSize, int pixels)
    {
        if (s.Length == 0)
            return new string[1] { "" };

        List<string> sa = new List<string>();

        int lastStart = 0;

        int d = CutIndex(s.Substring(lastStart), font, fontSize, pixels);
        for (int i = d; i < s.Length; i += d)
        {
            if (d < 2)
                break;
            
            bool found = false;
            //going back until the last whitespace or separator (or almost start of the row)
            for (int k = i; k > lastStart+5; --k)
            {
                if (char.IsWhiteSpace(s[k]) || char.IsSeparator(s[k]))
                {
                    sa.Add(s.Substring(lastStart, k - lastStart).Trim());
                    lastStart = k+1;
                    i = lastStart;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                sa.Add(s.Substring(lastStart, i - lastStart).Trim());
                lastStart = i;
            }

            d = CutIndex(s.Substring(lastStart), font, fontSize, pixels);
        }

        //final row
        sa.Add(s.Substring(lastStart, s.Length - lastStart));
        
        return sa.ToArray();
    }

    private static int ExtraHeightFromPages(int start = 0, bool toc = false)
    {
        if (!toc)
        {
            int totalHeight = 0;
            if (pageLengths != null)
            {
                for (int i = start; i < pageLengths.Count; ++i)
                {
                    totalHeight += pageLengths[i];
                }
            }

            return totalHeight;
        }
        else
        {
            int totalHeight = 0;
            if (tocPageLengths != null)
            {
                for (int i = start; i < tocPageLengths.Count; ++i)
                {
                    totalHeight += tocPageLengths[i];
                }
            }

            return totalHeight;
        }
    }
}