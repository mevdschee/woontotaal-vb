Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Module Program

    Sub Main(args As String())
        Dim Config as Config = New Config("config.json")
        Dim Api As WoonTotaal = new WoonTotaal(Config.Url, Config.ApiKey, Config.Company, Config.Username, Config.Password)
        Dim Materials As List(Of String) = Api.GetListOfMaterials("12")
        For Each s In Materials
            Console.WriteLine(s)
        Next
        Console.WriteLine("Press Enter to continue ...")
        Console.ReadLine()
    End Sub

End Module