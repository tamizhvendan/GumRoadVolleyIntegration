#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open FSharp.Data
let after = System.Environment.GetEnvironmentVariable("SALES_AFTER")
let baseUrl = "https://api.gumroad.com"
let gumRoadToken = System.Environment.GetEnvironmentVariable("GUMROAD_TOKEN")
let path = sprintf "/v2/sales?after=%s&access_token=%s" after gumRoadToken
let url = sprintf "%s%s" baseUrl

[<Literal>]
let SalesJsonResponse = """
{
  "success": true,
  "next_page_url": "/v2/sales?page=4&before=2015-09-03&after=2014-09-03",
  "sales": [{
    "product_id": "A-m3CDDC5dlrSdKZp0RFhA==",
    "created_at": "2015-06-30T17:  38:  02Z",
    "email": "calvin@gumroad.com",
    "custom_fields": {"First Name": "@gumroad", "Last Name" : "@test"},
    "price": 500
  }]
}
"""

type SalesJson = JsonProvider<SalesJsonResponse>
let printAndReturn x =
  printfn "%A" x
  x

let res = 
  url path
  |> Http.RequestString
  |> SalesJson.Parse

let volleyContactImportFormat (sale : SalesJson.Sale) = 
  sprintf "%s,%s,%s" sale.Email sale.CustomFields.FirstName sale.CustomFields.LastName

let content = 
  res.Sales
  |> Seq.map volleyContactImportFormat
  |> String.concat System.Environment.NewLine

System.IO.File.WriteAllText("import.csv", content)