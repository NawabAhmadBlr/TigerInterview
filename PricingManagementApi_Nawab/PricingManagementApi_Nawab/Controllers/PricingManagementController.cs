using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Route("api/pricing")]
[ApiController]
public class PricingController : ControllerBase
{
    private static List<PricingRecord> _pricingRecords = new List<PricingRecord>();

    [HttpPost("upload")]
    public async Task<IActionResult> UploadPricingRecords()
    {
        try
        {
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var uploadedRecords = new List<PricingRecord>();
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = line.Split(',');
                    if (values.Length == 5)
                    {
                        var record = new PricingRecord
                        {
                            StoreId = Convert.ToInt32(values[0]),
                            Sku = values[1],
                            ProductName = values[2],
                            Price = Convert.ToDecimal(values[3]),
                            Date = values[4]
                        };
                        uploadedRecords.Add(record);
                    }
                }

                _pricingRecords.AddRange(uploadedRecords);

                return Ok(uploadedRecords);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public IActionResult SearchPricingRecords([FromQuery] string criteria)
    {
        try
        {
            var results = _pricingRecords.FindAll(record =>
                record.StoreId.ToString().Contains(criteria) ||
                record.Sku.Contains(criteria) ||
                record.ProductName.Contains(criteria) ||
                record.Price.ToString().Contains(criteria) ||
                record.Date.Contains(criteria)
            );

            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class PricingRecord
{
    public int StoreId { get; set; }
    public string Sku { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Date { get; set; }
}
