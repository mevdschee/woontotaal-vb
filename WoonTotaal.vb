Option Strict On

Imports System.IO
Imports System.Net
Imports Newtonsoft.Json.Linq

Module WoonTotaal

    Class Api

        Private Url As String
        Private AccessToken As String
            
        Public Sub New(Url As String, ApiKey As String, Company As String, Username As String, Password As String)
            Me.Url = Url
            Me.AccessToken = GetAccessToken(ApiKey, Company, Username, Password)
        End Sub

        Private Function GetAccessToken(ApiKey As String, Company As String, Username As String, Password As String) As String 
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/token")
            myReq.Method = "POST"
            myReq.ContentType = "application/json"
            myReq.Headers.add("Api-Key",ApiKey)
            Dim myData As String = "{""domainName"":""" & Company & """,""username"":""" & Username & """,""password"":""" & Password & """}"
            myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myreader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myreader.ReadToEnd
            Dim myParsedText As JObject = JObject.Parse(myText)
            Return CStr(myParsedText.SelectToken("$.access_token"))
        End Function

        Public Function GetListOfMaterials(SearchText as String) As List(Of String) 
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/api/Gateway/Material/Browse")
            myReq.Method = "POST"
            myReq.ContentType = "application/json"
            myReq.Headers.add("Authorization", "bearer " & AccessToken)
            Dim myData As String = "{""filter"":{""description"":""" & SearchText & """}}"
            myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myReader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myReader.ReadToEnd
            Dim myParsedText As JObject = JObject.Parse(myText)
            Dim myModelNames As IEnumerable(Of JToken) = myParsedText.SelectTokens("$.items[*]")
            Dim myModelStrings As List(Of String) = New List(Of String)
            For Each myModelName as JToken In myModelNames
                Dim myDescription as JToken = myModelName.SelectToken("$.description")
                Dim myMaterialId as JToken = myModelName.SelectToken("$.id")
                Dim myIsWage as JToken = myModelName.SelectToken("$.isWage")
                If Not CBool(myIsWage) Then
                    myModelStrings.Add(Cstr(myDescription) & " / " & Cstr(myMaterialId))
                End If
            Next
            Return myModelStrings
        End Function

        Private Sub AddToListOfModels(myChildren as JArray, myModelStrings As List(Of String))
            For Each myObject as JObject In myChildren
                Dim nextChildren As JArray = JArray.FromObject(myObject.SelectToken("$.children"))
                Dim myDescription As JToken = myObject.SelectToken("$.fullDescription")
                Dim myModelId As JToken = myObject.SelectToken("$.id")
                If nextChildren IsNot Nothing AndAlso nextChildren.Count()>0 Then
                    AddToListOfModels(nextChildren,myModelStrings)
                Else
                    myModelStrings.Add(Cstr(myDescription) & " / " & Cstr(myModelId))
                End If
            Next
        End Sub

        Public Function GetListOfModels() As List(Of String) 
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/api/Gateway/Model/GetAll")
            myReq.Headers.add("Authorization", "bearer " & AccessToken)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myReader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myReader.ReadToEnd
            Dim myChildren As JArray = JArray.Parse(myText)
            Dim myModelStrings As List(Of String) = New List(Of String)
            AddToListOfModels(myChildren,myModelStrings)
            Return myModelStrings            
        End Function


        Public Function GetPropertiesForOrder(ModelId As Integer, MaterialId As Integer, Width As Integer, Height as Integer) As List(Of String)
            Dim myParams = "?modelId=" & CStr(ModelId) & "&materialId=" & CStr(MaterialId) & "&width=" & CStr(Width) & "&height=" & CStr(Height)
            Dim myReq As WebRequest = HttpWebRequest.Create(Url & "/api/Gateway/Project/CreateProjectWithPolygon" & myParams)
            myReq.Method = "POST"
            myReq.ContentType = "application/json"
            myReq.Headers.add("Authorization", "bearer " & AccessToken)
            Dim myData As String = "{}"
            myReq.GetRequestStream.Write(System.Text.Encoding.UTF8.GetBytes(myData), 0, System.Text.Encoding.UTF8.GetBytes(myData).Count)
            Dim myResp As WebResponse = myReq.GetResponse
            Dim myReader As New System.IO.StreamReader(myResp.GetResponseStream)
            Dim myText As String = myReader.ReadToEnd

'Dim objWriter As New System.IO.StreamWriter( "materials.json" )
'objWriter.Write( myText )
'objWriter.Close()

            Dim myChildren As JArray = JArray.Parse(myText)
            Dim myProperties As List(Of String)  = New List(Of String)
            Return myProperties            
        End Function

    End Class

End Module
