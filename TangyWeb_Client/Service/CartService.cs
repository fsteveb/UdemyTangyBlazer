using System;
using TangyWeb_Client.Service.IService;
using TangyWeb_Client.ViewModels;
using Blazored.LocalStorage;
using Tangy_Common;

namespace TangyWeb_Client.Service
{
	public class CartService : ICartService
	{
		public ILocalStorageService _localStorage { get; set; }
		public event Action OnChange;

		public CartService(ILocalStorageService LocalStorage)
		{
			_localStorage = LocalStorage;
		}

		public async Task DecrementCart(ShoppingCart cartToDecrement)
		{
			var cart = await _localStorage.GetItemAsync<List<ShoppingCart>>(SD.ShoppingCart);

			// if count is 0 or 1 then remove item
			for (int i = 0; i < cart.Count; i++)
			{
				if (cart[i].ProductId == cartToDecrement.ProductId && cart[i].ProductPriceId == cartToDecrement.ProductPriceId)
				{
					if (cart[i].Count == 1 || cartToDecrement.Count == 0)
					{
						cart.Remove(cart[i]);
					}
					else
					{
						cart[i].Count -= cartToDecrement.Count;
					}
				}
			}

			await _localStorage.SetItemAsync(SD.ShoppingCart, cart);
			OnChange.Invoke();
		}

		public async Task IncrementCart(ShoppingCart cartToAdd)
		{
			var cart = await _localStorage.GetItemAsync<List<ShoppingCart>>(SD.ShoppingCart);
			bool ItemInCart = false;
			if (cart == null)
			{
				cart = new List<ShoppingCart> { cartToAdd };
			}

			foreach (var obj in cart)
			{
				if (obj.ProductId == cartToAdd.ProductId && obj.ProductPriceId == cartToAdd.ProductPriceId)
				{
					ItemInCart = true;
					obj.Count += cartToAdd.Count;
				}
            }

            if (!ItemInCart)
			{
				cart.Add(new ShoppingCart()
				{
					ProductId = cartToAdd.ProductId,
					ProductPriceId = cartToAdd.ProductPriceId,
					Count = cartToAdd.Count
				});
			}

			await _localStorage.SetItemAsync(SD.ShoppingCart, cart);
            OnChange.Invoke();
        }
    }
}
