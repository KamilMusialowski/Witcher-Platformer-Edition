using System.IO;
using UnityEditor;
using UnityEngine;

public class PDFCreatorUI : EditorWindow
{
    Vector2 scrollPosIntro;
    Vector2 scrollPos;
    static PDFCreatorUI window;
    static Texture2D logo;

    [MenuItem("PDF Documentation/Generate Documentation")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        if (logo == null)
        {
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/PDF Documentation/Editor/logo.png");
        }
        window = (PDFCreatorUI)GetWindow(typeof(PDFCreatorUI), true, "PDF Documentation", true);
        window.maxSize = new Vector2(600, 1500);
        window.minSize = new Vector2(600, 500);
        PdfCreator.InitEditorVariables();
        window.Show();
    }
    
    public void OnGUI()
    {
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayout.BeginVertical(GUILayout.Height((Screen.width - 60) / 4f + 50));
        GUI.DrawTexture(new Rect(24, 24, Screen.width - 60, (Screen.width - 60)/4f), logo, ScaleMode.ScaleToFit);
        GUILayout.Space((Screen.width - 60) / 4f + 50);
        GUILayout.EndVertical();
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        PdfCreator.PDFName = EditorGUILayout.TextField(new GUIContent("PDF file name", "The file name that the PDF documentation will have. Default value is 'Documentation'."), PdfCreator.PDFName);
        EditorGUILayout.Space();
        PdfCreator.DocumentTitle = EditorGUILayout.TextField(new GUIContent("Document title", "The document title is displayed at the start of the first page of the PDF document. Default value is the project's product name."), PdfCreator.DocumentTitle);
        EditorGUILayout.Space();
        PdfCreator.PDFToRoot = EditorGUILayout.Toggle(new GUIContent("PDF to project root", "Create the PDF document to project root folder specified under Script Reference settings. If this is false, you can specify another path for the PDF. Default value is true."), PdfCreator.PDFToRoot);
        EditorGUILayout.Space();
        if (!PdfCreator.PDFToRoot)
        {
            EditorGUILayout.BeginHorizontal();
            PdfCreator.PDFFolder = EditorGUILayout.TextField(new GUIContent("PDF path", "The path where the PDF document will be created."), PdfCreator.PDFFolder);
            if (GUILayout.Button("Change", GUILayout.Width(100)))
            {
                string path = EditorUtility.OpenFolderPanel("PDF folder", PdfCreator.PDFFolder, "");
                if (path != null && path.Length > 0)
                    PdfCreator.PDFFolder = path;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        DrawLine();

        GUILayout.Label("Script Reference", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        PdfCreator.ProjectRoot = EditorGUILayout.TextField(new GUIContent("Project root", "The root folder for the project. The script reference will go through this folder and it's contents recursively. Default value is the project's Assets-folder. The root folder can be outside of this project."), PdfCreator.ProjectRoot);
        if (GUILayout.Button("Change", GUILayout.Width(100)))
        {
            string path = EditorUtility.OpenFolderPanel("Project root", PdfCreator.ProjectRoot, "");
            if (path != null && path.Length > 0)
                PdfCreator.ProjectRoot = path;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        PdfCreator.IncludePrivates = EditorGUILayout.Toggle(new GUIContent("Non-public elements", "When this option is off, only elements that are defined as public will be included in the script reference. Default value is true."), PdfCreator.IncludePrivates);
        EditorGUILayout.Space();
        PdfCreator.IncludeEmpty = EditorGUILayout.Toggle(new GUIContent("Empty methods", "When this option is off, only methods that contain something other than comments will be included in the script reference. Default value is true."), PdfCreator.IncludeEmpty);
        EditorGUILayout.Space();
        PdfCreator.IncludeShaders = EditorGUILayout.Toggle(new GUIContent("Shader files", "Include .shader and .compute files in the script reference. The contents or functionality of the shader will not be included. Default value is true."), PdfCreator.IncludeShaders);
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(!PdfCreator.IncludeShaders);
        PdfCreator.ShaderDescription = EditorGUILayout.Toggle(new GUIContent("Shader description", "Any comments at the start of a shader file will be added as the file's description. Default value is true."), PdfCreator.ShaderDescription);
        EditorGUILayout.Space();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        PdfCreator.IgnoreFolders = EditorGUILayout.TextField(new GUIContent("Ignore folders containing", "All folder names containing any of the strings that are listed here (separated by commas), will not be included in the script reference. The field is case-sensitive. The contents of the folders will also be excluded. Use quotation marks (\"Example\") for exact folder names."), PdfCreator.IgnoreFolders);
        EditorGUILayout.Space();
        PdfCreator.IgnoreFiles = EditorGUILayout.TextField(new GUIContent("Ignore files containing", "All files names containing any of the strings that are listed here (separated by commas), will not be included in the script reference. The field is case-sensitive. The contents of the files will also be excluded. Use quotation marks (\"Example\") for exact file names."), PdfCreator.IgnoreFiles);
        EditorGUILayout.Space();

        GUILayout.Label(new GUIContent("Table of Contents", "Table of Contents for the script reference. This will be displayed at the start of the document, and the names work as hyperlinks to that entry. Entries can still be included in the script reference even if they are not in the Table of Contents."), EditorStyles.miniBoldLabel);
        PdfCreator.TocIncludeRootFolder = EditorGUILayout.Toggle(new GUIContent("Root Folder", "Include the project root folder in the Table of Contents and the script reference. Default value is true."), PdfCreator.TocIncludeRootFolder);
        PdfCreator.TocIncludeFolders = EditorGUILayout.Toggle(new GUIContent("Folders", "Include folders in the Table of Contents. The files under the folders can still be included even if folders are not. Default value is true."), PdfCreator.TocIncludeFolders);
        PdfCreator.TocIncludeFiles = EditorGUILayout.Toggle(new GUIContent("Files", "Include files in the Table of Contents. The content inside the files can still be included even if files are not. Default value is true."), PdfCreator.TocIncludeFiles);
        PdfCreator.TocIncludeNamespaces = EditorGUILayout.Toggle(new GUIContent("Namespaces", "Include the names of the namespaces in the Table of Contents. Default value is false."), PdfCreator.TocIncludeNamespaces);
        PdfCreator.TocIncludeClasses = EditorGUILayout.Toggle(new GUIContent("Classes", "Include classes and enumerations in the Table of Contents. Default value is false."), PdfCreator.TocIncludeClasses);
        PdfCreator.TocIncludeMethods = EditorGUILayout.Toggle(new GUIContent("Methods", "Include methods, delegates, constructors and destructors in the Table of Contents. Default value is false."), PdfCreator.TocIncludeMethods);
        PdfCreator.TocIncludeVariables = EditorGUILayout.Toggle(new GUIContent("Variables", "Include variables and properties in the Table of Contents. Default value is false."), PdfCreator.TocIncludeVariables);

        EditorGUILayout.Space();

        DrawLine();

        GUILayout.Label(new GUIContent("Introduction text and step-by-step setup guide", "Unity Asset Store assets that include a setup process to get the asset working should include a step-by-step setup guide. This guide will be visible at the start of the document. The field can be used for other purposes too, such as introduction text."), EditorStyles.boldLabel);

        GUIStyle introAreaStyle = EditorStyles.textArea;
        GUIStyle introTextStyle = EditorStyles.textArea;
        introTextStyle.wordWrap = true;
        EditorGUILayout.BeginHorizontal(introAreaStyle);
        scrollPosIntro = EditorGUILayout.BeginScrollView(scrollPosIntro, GUILayout.MinHeight(150));
        PdfCreator.IntroductionText = EditorGUILayout.TextArea(PdfCreator.IntroductionText, introTextStyle, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();


        EditorGUILayout.Space();

        DrawLine();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button(new GUIContent("Reset to default", "Resets all of the above fields to their default values."), GUILayout.Height(30), GUILayout.Width(150)))
        {
            if (EditorUtility.DisplayDialog("Confirm reset", "Are you sure you want to reset the values? This action cannot be reversed.", "Yes", "No"))
            {
                PdfCreator.ResetEditorValues();
            }
        }
        EditorGUILayout.Space();
        if (PdfCreator.ActiveJob)
        {
            if (GUILayout.Button(new GUIContent("Stop", "Stops the PDF documentation creation process"), GUILayout.Height(30), GUILayout.Width(150)))
            { 
                PdfCreator.Stop();
                Debug.Log("PDF creation stopped.");
            }
        }
        else
        {
            if (GUILayout.Button(new GUIContent("Create PDF", "Creates the PDF documentation file with the given information."), GUILayout.Height(30), GUILayout.Width(150)))
            {
                PdfCreator.CreateDocument();
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        DrawLine();
        EditorGUILayout.Space();
        GUIStyle statusStyle = new GUIStyle();
        if (PdfCreator.StatusTextGood)
            statusStyle.normal.textColor = new Color(0.1f, 0.1f, 0.5f); 
        else
            statusStyle.normal.textColor = new Color(0.5f, 0.1f, 0.1f);
        EditorGUILayout.LabelField(PdfCreator.StatusText, statusStyle);
        EditorGUILayout.Space();

        if (PdfCreator.ActiveJob)
        {
            string infoText = "";
            if (PdfCreator.FilesParsed == PdfCreator.TotalFiles)
                infoText = "Creating document...";
            else if (PdfCreator.FilesParsed < PdfCreator.TotalFiles)
            {
                infoText = "Parsing file " + (PdfCreator.FilesParsed + 1) + " of " + PdfCreator.TotalFiles;
            }
            else
            {
                infoText = "Finalizing the document...";
            }
            EditorUtility.DisplayProgressBar("PDF Generation progress", infoText, PdfCreator.progress);
        }
        else
            EditorUtility.ClearProgressBar();

        GUI.EndGroup();
    }

    void DrawLine(int height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, height);
        rect.height = height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f));

    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}
