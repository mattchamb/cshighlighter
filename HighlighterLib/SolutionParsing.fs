module SolutionParsing

open System
open System.Collections.Generic
open System.IO
open System.Reflection

// Copy the solution parsing code from here (http://stackoverflow.com/questions/707107/library-for-parsing-visual-studio-solution-files)
// and translate it into F#

type SolutionProject(solutionProject: Object) =
    static let s_ProjectInSolution = Type.GetType("Microsoft.Build.Construction.ProjectInSolution, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false, false)
    static do
        if s_ProjectInSolution = null then
            failwith "Could not load ProjectInSolution type."
    static let s_ProjectInSolution_ProjectName = s_ProjectInSolution.GetProperty("ProjectName", BindingFlags.NonPublic ||| BindingFlags.Instance)
    static let s_ProjectInSolution_RelativePath = s_ProjectInSolution.GetProperty("RelativePath", BindingFlags.NonPublic ||| BindingFlags.Instance)
    static let s_ProjectInSolution_ProjectGuid = s_ProjectInSolution.GetProperty("ProjectGuid", BindingFlags.NonPublic ||| BindingFlags.Instance)

    let projectName = s_ProjectInSolution_ProjectName.GetValue(solutionProject, null) :?> string
    let relativePath = s_ProjectInSolution_RelativePath.GetValue(solutionProject, null) :?> string
    let projectGuid = s_ProjectInSolution_ProjectGuid.GetValue(solutionProject, null) :?> string

    member this.ProjectName with get () = projectName
    member this.RelativePath with get () = relativePath
    member this.ProjectGuid with get () = projectGuid

type Solution(contents: string) =
    //internal class SolutionParser
    //Name: Microsoft.Build.Construction.SolutionParser
    //Assembly: Microsoft.Build, Version=4.0.0.0
    static let s_SolutionParser = Type.GetType("Microsoft.Build.Construction.SolutionParser, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false, false)
    static do
        if s_SolutionParser = null then
            failwith "Could not load SolutionParser type."
    static let s_SolutionParser_solutionReader = s_SolutionParser.GetProperty("SolutionReader", BindingFlags.NonPublic ||| BindingFlags.Instance)
    static let s_SolutionParser_parseSolution = s_SolutionParser.GetMethod("ParseSolution", BindingFlags.NonPublic ||| BindingFlags.Instance)
    static let s_SolutionParser_projects = s_SolutionParser.GetProperty("Projects", BindingFlags.NonPublic ||| BindingFlags.Instance)
    
    let solutionParser = s_SolutionParser.GetConstructors(BindingFlags.Instance ||| BindingFlags.NonPublic).[0].Invoke(null)
    do
        use str = new StringReader(contents)
        s_SolutionParser_solutionReader.SetValue(solutionParser, str, null)
        s_SolutionParser_parseSolution.Invoke(solutionParser, null) |> ignore
    let projects = new List<SolutionProject>();
    let array = s_SolutionParser_projects.GetValue(solutionParser, null) :?> System.Array
    do
        for i in 0 .. (array.Length - 1) do
            projects.Add(new SolutionProject(array.GetValue(i)));
    let p = Seq.toList projects 

    member this.Projects with get () = p
