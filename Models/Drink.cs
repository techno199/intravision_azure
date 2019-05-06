using System.ComponentModel.DataAnnotations;

namespace server.Models
{
  public class Drink
  {
    public int Id { get; set; }
    public string Name { get; set; }
    [ConcurrencyCheck]
    public int Price { get; set; }
    [ConcurrencyCheck]
    public int Quantity { get; set; }
    public Device Device { get; set; } 
  }
}