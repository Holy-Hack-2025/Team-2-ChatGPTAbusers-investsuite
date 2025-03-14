﻿using Backend.Data;
using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : Controller
    {
        private readonly BackendDbContext _db;
        private readonly HttpClient _httpClient;

        public StockController(BackendDbContext db, IConfiguration configuration)
        {
            _db = db;
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> SetStock(string token, int amount)
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
                return Unauthorized("User ID not found in token.");

            //if exists
            var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.Token == token);
            if (stock == null)
            {
                //validate that the stock is real
                var stockInfo = await GetStockInfo(token);

                if (stockInfo == null)
                {
                    return BadRequest("no info found on stock");
                }

                //create
                stock = new() { Token = stockInfo.Token, UserId = userId.Value, Amount = amount, Name = stockInfo.Name };
                await _db.Stocks.AddAsync(stock);
                _db.SaveChanges();
                return Ok(stock);
            }

            if (amount == 0)
            {
                //delete
                _db.Stocks.Remove(stock);
                _db.SaveChanges();
                return Ok();
            }

            //update
            stock.Amount = amount;
            _db.SaveChanges();
            return Ok(stock);

        }

        [HttpGet]
        public async Task<IActionResult> GetStocks()
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
                return Unauthorized("User ID not found in token.");


            var userStocks = _db.Stocks.Where(x => x.UserId == userId).ToList();

            return Ok(userStocks);
        }

        [HttpGet("{token}")]
        public async Task<ActionResult<StockInfo>> GetStockHistory(string token)
        {
            var data = await GetStockInfo(token);

            if (data == null) return BadRequest();

            return Ok(data);
        }

        [HttpGet("question")]
        public async Task<IActionResult> GetQuestion()
        {
            var userId = GetUserIdFromToken();

            if (userId == null)
                return Unauthorized("User ID not found in token.");


            var userStocks = _db.Stocks.Where(x => x.UserId == userId).OrderBy(x => Guid.NewGuid()).ToList();

            if (userStocks.Count < 4)
            {
                return BadRequest("You need more stocks");
            }

            var tasks = userStocks
                        .Select(async stock => await GetStockInfo(stock.Token))
                        .ToList();

            var stockInfos = await Task.WhenAll(tasks);
            var stocks = stockInfos.Where(s => s != null).ToList();

            if (stocks.Count < 4)
            {
                return BadRequest("You need more real stocks");
            }

            stocks = stocks.Take(4).ToList();
            var randomStock = stocks.OrderBy(x => Guid.NewGuid()).First();

            var question = new
            {
                Token = randomStock?.Token,
                Options = stocks,
                Name = randomStock?.Name
            };

            return Ok(question);
        }

        private async Task<StockInfo?> GetStockInfo(string token)
        {
            string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{token}?interval=1d&range=30d";

            try
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw exception if not 200 OK

                string jsonResponse = await response.Content.ReadAsStringAsync();

                JObject jsonObject = JObject.Parse(jsonResponse);

                var test = jsonObject.SelectToken("chart.result[0].meta.currency");

                var data = new StockInfo()
                {
                    Currency = jsonObject.SelectToken("chart.result[0].meta.currency").ToString(),
                    Token = jsonObject.SelectToken("chart.result[0].meta.symbol").ToString(),
                    RegularMarketPrice = float.Parse(jsonObject.SelectToken("chart.result[0].meta.regularMarketPrice").ToString()),
                    Name = jsonObject.SelectToken("chart.result[0].meta.shortName").ToString()
                };

                var historicPrices = jsonObject.SelectToken("chart.result[0].indicators.quote[0].close").Values();
                foreach (var jsonPrice in historicPrices)
                {
                    data.HistoricPrices.Add(float.Parse(jsonPrice.ToString()));
                }

                return data;
            }
            catch
            {
                return null;
            }
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            if (int.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
    }
}
