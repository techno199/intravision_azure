using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using server.Services;
using server.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace server.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class CoinsController : ControllerBase
  {
    private readonly DbService _dbService;

    public CoinsController(
      DbService dbService
    )
    {
      _dbService = dbService;
    }

    [HttpGet]
    public async Task<List<Coin>> Get(
      [FromQuery]int deviceId
    )
    {
      return await _dbService.GetCoins(deviceId);
    }


    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Coin>> Post(
      [FromBody]Coin coin
    )
    {
      var deviceId = Int32.Parse(User.Identity.Name);
      var newCoin = await _dbService.CreateCoin(coin, deviceId);
      if (newCoin != null)
        return newCoin;
      return BadRequest();
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<Coin>> Put(
      [FromBody]Coin coin
    )
    {
      var updatedCoin = await _dbService.UpdateCoin(coin);
      if (updatedCoin != null)
      {
        return updatedCoin;
      }
      return BadRequest();
    }

    [HttpDelete]
    [Authorize]
    public async Task Delete(
      [FromQuery]int coinId
    )
    {
      await _dbService.DeleteCoin(coinId);
    }

    [HttpGet]
    [Route("[action]")]
    public async Task ChangeAmount(
      [FromQuery]int coinId,
      [FromQuery]int amount
    )
    {
      await _dbService.ChangeCoinAmount(coinId, amount);
    }

    // In real world this method must be authorized and secured
    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult<List<Coin>>> GrabChange(
      [FromQuery]int deviceId,
      [FromQuery]int change)
    {
      try
      {
        var coins = await _dbService.GrabChange(deviceId, change);
        if (coins == null)
        {
          return BadRequest("No device was found");
        }

        // Temp workaround for lazy loading
        foreach (var coin in coins)
        {
          coin.Device = null;
        }

        return coins;
      }
      catch
      {
        return BadRequest("An error occured while trying to pick up change");
      }
    }
  }
}