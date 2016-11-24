#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open FSharp.Data
let after = System.Environment.GetEnvironmentVariable("SALES_AFTER")
let baseUrl = "https://api.gumroad.com"
let gumRoadToken = System.Environment.GetEnvironmentVariable("GUMROAD_TOKEN")
let path = sprintf "/v2/sales?after=%s" after 
let url (path : string) = 
  let queryStringChar = if path.Contains("?") then "&" else "?"
  sprintf "%s%s%saccess_token=%s" baseUrl path queryStringChar gumRoadToken

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


let rec getSales xs path =
  let sales = url path |> Http.RequestString |> SalesJson.Parse
  if sales.NextPageUrl = "" then
    xs
  else
    let agg = Array.concat [sales.Sales;xs] 
    printfn "%A" sales.NextPageUrl
    getSales agg sales.NextPageUrl 


let res = 
  url path
  |> Http.RequestString
  |> SalesJson.Parse

let volleyContactImportFormat (sale : SalesJson.Sale) = 
  sprintf "%s,%s,%s" sale.Email sale.CustomFields.FirstName sale.CustomFields.LastName

let content = 
  getSales Array.empty path
  |> Seq.map volleyContactImportFormat
  |> String.concat System.Environment.NewLine

System.IO.File.WriteAllText("import.csv", content)