﻿using Newtonsoft.Json;
using Tangy_Models;
using TangyWeb_Client.Service.IService;

namespace TangyWeb_Client.Service
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string BaseServerUrl;

        public OrderService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            BaseServerUrl = _configuration.GetSection("BaseServerUrl").Value;
        }

        public async Task<OrderDTO> Get(int orderHeaderID)
        {
            var response = await _httpClient.GetAsync($"/api/order/orderHeaderID?orderHeaderID={orderHeaderID}");
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var order = JsonConvert.DeserializeObject<OrderDTO>(content);
                return order;
            }
            else
            {
                var errorModel = JsonConvert.DeserializeObject<ErrorModelDTO>(content);
                throw new Exception(errorModel.ErrorMessage);
            }
        }

        public async Task<IEnumerable<OrderDTO>> GetAll(string? userId = null)
        {
            var response = await _httpClient.GetAsync("/api/order");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<IEnumerable<OrderDTO>>(content);

                return orders;
            }

            return new List<OrderDTO>();
        }
    }
}
