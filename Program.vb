Option Strict On

Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Module Program

    Public Sub Main(args As String())
        Dim Config as Utils.Config = New Utils.Config("config.json")
        Dim Api As WoonTotaal.Api = new WoonTotaal.Api(Config.Value("Url"), Config.Value("ApiKey"), Config.Value("Company"), Config.Value("Username"), Config.Value("Password"))
        'Dim Materials As List(Of String) = Api.GetListOfMaterials("5230")
        'For Each s In Materials
        '    Console.WriteLine(s)
        'Next
        'Console.WriteLine("Press Enter to continue ...")
        'Console.ReadLine()
        'Dim Models As List(Of String) = Api.GetListOfModels()
        'For Each s In Models
        '    Console.WriteLine(s)
        'Next
        'Console.WriteLine("Press Enter to continue ...")
        'Console.ReadLine()
        Dim Properties As List(Of String) = Api.GetPropertiesForOrder(8114,55406,400,250)
        For Each s In Properties
            Console.WriteLine(s)
        Next
        Console.WriteLine("Press Enter to continue ...")
        Console.ReadLine()
    End Sub

End Module