using server.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace server.Services
{
  public class DbService
  {
    private readonly Context _context;
    public DbService(
      Context context
    )
    {
      _context = context;
    }

    public async Task<bool> IsAuthorized(int deviceId, string code)
    {
      var target = await _context.Devices.FindAsync(deviceId);
      if (target.AdminCode == code)
        return true;
      return false;
    }

    public Task<List<Drink>> GetDrinks(int deviceId)
    {
      var drinks = _context.Drinks
        .Where(d => d.Device.Id == deviceId)
        .ToListAsync();
      
      return drinks;
    }

    public async Task<Drink> CreateDrink(Drink drink, int deviceId)
    {
      var device = await _context.Devices
        .FindAsync(deviceId);

      if (device == null)
        return null;

      device.Drinks
        .Add(drink);

      await _context.SaveChangesAsync();
      // Prevent navigation entities from being presented in response
      drink.Device = null;
      return drink;
    }

    public async Task DeleteDrink(int drinkId)
    {
      _context.Drinks.
        Remove(
          await _context.Drinks.FindAsync(drinkId)
        );
      await _context.SaveChangesAsync();
    }

    public async Task<Drink> UpdateDrink(Drink drink)
    {
      var target = await _context.Drinks.FindAsync(drink.Id);
      target.Price = drink.Price;
      target.Quantity = drink.Quantity;
      await _context.SaveChangesAsync();
      target.Device = null;
      return target;
    } 

    public async Task BuyDrink(int drinkId, int amount)
    {
      var drink = await _context.Drinks.FindAsync(drinkId);
      if (drink.Quantity == 0)
        throw new Exception("Drink is over");
      drink.Quantity -= amount;
      await _context.SaveChangesAsync();
    }

    public Task<List<Coin>> GetCoins(int deviceId)
    {
      return _context.Coins
        .Where(c => c.Device.Id == deviceId)
        .ToListAsync();
    }

    public async Task<Coin> CreateCoin(Coin coin, int deviceId)
    {
      var device = await _context.Devices
        .FindAsync(deviceId);

      _context.Entry(device).Collection(d => d.Coins).Load();

      if (device.Coins.FirstOrDefault(c => c.Value == coin.Value) == null)
      {
        device.Coins.Add(coin);
        await _context.SaveChangesAsync();
        coin.Device = null;
        return coin;
      }
      return null;
    }

    public async Task<Coin> UpdateCoin(Coin coin)
    {
      var target = await _context.Coins
        .FirstAsync(c => c.Id == coin.Id);
      if (target == null)
      {
        target.Value = coin.Value;
        target.Quantity = coin.Quantity;
        target.IsBlocked = coin.IsBlocked;

        await _context.SaveChangesAsync();
        return coin;
      }
      return null;
    }

    public async Task DeleteCoin(int coinId)
    {
      _context.Coins
        .Remove(
          await _context.Coins.FindAsync(coinId)
        );
      await _context.SaveChangesAsync();
    }

    public async Task ChangeCoinAmount(int coinId, int amount)
    {
      var coin = await _context.Coins.FindAsync(coinId);
      coin.Quantity += amount;
      await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Pick up change for given cash.
    /// </summary>
    /// <param name="deviceId">device id</param>
    /// <param name="change">change to pick up</param>
    /// <returns>New coin state</returns>
    public async Task<List<Coin>> GrabChange(int deviceId, int change)
    {
      var device = await _context.Devices.FindAsync(deviceId);
      // Return null if no devices found
      if (device == null)
      {
        return null;
      }

      var coins = await _context.Entry(device)
        .Collection(d => d.Coins)
        .Query()
        .OrderByDescending(c => c.Value)
        .ToListAsync();

      foreach (var coin in device.Coins)
      {
        var requiredQuantity = Math.Min(
          coin.Quantity,
          (int)Math.Floor((decimal)change / coin.Value));

        coin.Quantity -= requiredQuantity;
        change -= requiredQuantity * coin.Value;

        if (change == 0)
        {
          await _context.SaveChangesAsync();
          return coins;
        }
      }

      throw new Exception("Can't pick up change");
    }
  }
}