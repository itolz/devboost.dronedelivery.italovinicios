using Geolocation;
using grupo4.devboost.dronedelivery.Data;
using grupo4.devboost.dronedelivery.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace grupo4.devboost.dronedelivery.Services
{
    public class PedidoService : IPedidoService
    {
        const double latitudeBaseDrone = -23.5880684;
        const double longitudeBaseDrone = -46.6564195;

        private readonly IDroneService _droneService;

        public PedidoService(IDroneService droneService)
        {
            _droneService = droneService;
        }

        public grupo4devboostdronedeliveryContext context { get; set; }

        public async Task<DroneDTO> DroneAtendePedido(Pedido pedido)
        {

            double distance = GeoCalculator.GetDistance(latitudeBaseDrone, longitudeBaseDrone, pedido.Latitude, pedido.Longitude, 1, DistanceUnit.Kilometers) * 2;

            var drones = await _droneService.GetAll();

            var buscaDrone = drones.Where(d => d.Perfomance >= distance && d.Capacidade >= pedido.Peso).FirstOrDefault();

            if (buscaDrone == null)
                return null;

            return new DroneDTO(buscaDrone, distance);

        }

        public bool DroneAtendeMaisUmPedido(Drone drone, Pedido novoPedido,  List<Pedido> sacolaPedidosDrone)
        {

            var pedidosOrdenados = sacolaPedidosDrone.OrderBy(o => o.DataHoraInclusao);
            double distanciaTotal = 0;
            int pesoTotal = 0; 
            double latitudePontoParada = 0;
            double longitutePontoParada = 0;

            foreach (var pedido in pedidosOrdenados)
            {
                
                if (distanciaTotal == 0) //primeira viagem
                {
                    distanciaTotal = GeoCalculator.GetDistance(latitudeBaseDrone, longitudeBaseDrone, pedido.Latitude, pedido.Longitude, 1, DistanceUnit.Kilometers);
                }
                else
                { 
                    distanciaTotal += GeoCalculator.GetDistance(latitudePontoParada, longitutePontoParada, pedido.Latitude, pedido.Longitude, 1, DistanceUnit.Kilometers);
                }

                latitudePontoParada = pedido.Latitude;
                longitutePontoParada = pedido.Longitude;

                pesoTotal += pedido.Peso;

            }


            return 

      
        }
    }
}
