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
                    myModelStrings.Add(Cstr(myDescription) & " ~ " & Cstr(myMaterialId))
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
                    myModelStrings.Add(Cstr(myDescription) & " ~ " & Cstr(myModelId))
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

        Private Function GetDataType(DataType as Integer) As String
            Dim myTypes As Dictionary(Of Integer, String) = New Dictionary(Of Integer, String)()
            myTypes.Add(1,"label")
            myTypes.Add(2,"double")
            myTypes.Add(4,"integer")
            myTypes.Add(10,"boolean")
            myTypes.Add(11,"string")
            Return myTypes(DataType)
        End Function

        Private Function GetListValues(myProperty as JToken) As Dictionary(Of String, String)
            Dim myListValues As IEnumerable(Of JToken) = myProperty.SelectTokens("$.listValues[*]")
            Dim myListValueStrings As Dictionary(Of String, String) = New Dictionary(Of String, String)()
            If myListValues IsNot Nothing
                For Each myListValue as JToken In myListValues
                    Dim myDescription as JToken = myListValue.SelectToken("$.description")
                    Dim myValue2 as JToken = myListValue.SelectToken("$.value")
                    myListValueStrings(CStr(myValue2)) = CStr(myDescription)
                Next
            End If
            Return myListValueStrings
        End Function

        Private Function GetPropertyString(myProperty as JToken) As String
            Dim myDisplayLabel as JToken = myProperty.SelectToken("$.displayLabel")
            Dim myValue as JToken = myProperty.SelectToken("$.value")
            Dim myDisplaySuffix as JToken = myProperty.SelectToken("$.displaySuffix")
            Dim myDataType as JToken = myProperty.SelectToken("$.dataType")
            Dim myListValueStrings As Dictionary(Of String, String) = GetListValues(myProperty)
            Dim myValueString As String = Cstr(myValue)
            If myListValueStrings.ContainsKey(myValueString) Then
                myValueString = myListValueStrings(myValueString)
            End If
            Dim myListValueString As String = String.Join("`", myListValueStrings.Values)
            Return Cstr(myDisplayLabel) & "~" & myValueString & "~" & Cstr(myDisplaySuffix) & "~" & GetDataType(CInt(myDataType)) & "~" & myListValueString
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
            Dim myParsedText As JObject = JObject.Parse(myText)
            Dim myProperties As IEnumerable(Of JToken) = myParsedText.SelectTokens("$.polygons[0].categoryLinks[0].categoryLinkMaterial.properties[*]")
            Dim myPropertyStrings As List(Of String) = New List(Of String)
            For Each myProperty as JToken In myProperties
                myPropertyStrings.Add(GetPropertyString(myProperty))
            Next

            Dim myPriceGroups As IEnumerable(Of JToken) = myParsedText.SelectTokens("$.polygons[0].prices.priceGroups[*]")
            For Each myPriceGroup as JToken In myPriceGroups
                Dim myCode as JToken = myPriceGroup.SelectToken("$.code")
                Dim myUnitName as String = "Arbeid"
                If CStr(myCode) = "MATERIAL" Then
                    myUnitName = "Artikel"
                End If
                Dim myElements As IEnumerable(Of JToken) = myPriceGroup.SelectTokens("$.elements[*]")
                For Each myElement as JToken In myElements
                    Dim myQuantity as JToken = myElement.SelectToken("$.quantity")
                    Dim myUnit as JToken = myElement.SelectToken("$.unit")
                    Dim myDescription as JToken = myElement.SelectToken("$.description")
                    Dim myUnitPrice as JToken = myElement.SelectToken("$.unitPrice")
                    Dim myTotalPrice as JToken = myElement.SelectToken("$.totalPrice")
                    Dim myLine As String = Cstr(myQuantity) & "`" & Cstr(myUnit) & "`" & Cstr(myDescription) & "`" & Cstr(myUnitPrice) & "`" & Cstr(myTotalPrice)
                    myPropertyStrings.Add("Offerteregel~" & myLine & "~" & myUnitName & "~label~")
                Next
            Next
            'Dim objWriter As New System.IO.StreamWriter("properties.json")
            'objWriter.Write( myText )
            'objWriter.Close()
            Return myPropertyStrings
        End Function

    End Class

End Module
