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

        public async Task<DroneDTO> AtribuirPedidoDrone(Pedido pedido)
        {
            var drones = await _droneService.GetAll();

            foreach (var drone in drones)
            {
                if (DroneAtendeMaisUmPedido(drone, pedido)) return new DroneDTO(drone, 0); //abandona a rotina quando encontra um drone disponivel
            }

            return null;
        }

        public bool DroneAtendeMaisUmPedido(Drone drone, Pedido novoPedido)
        {

            //adiciona o novo pedido na sacola para calculos
            if (drone.pedidos == null) drone.pedidos = new List<Pedido>();
          
                drone.pedidos.Add(novoPedido);


            var pedidosOrdenados = drone.pedidos.OrderBy(o => o.DataHoraInclusao);
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

            //adiciona o trecho de volta a base
            distanciaTotal += GeoCalculator.GetDistance(latitudePontoParada, longitutePontoParada, latitudeBaseDrone, longitudeBaseDrone, 1, DistanceUnit.Kilometers);

            //calcula o tempo total de viagem e seta o tempo final 
            novoPedido.DataHoraFinalizacao = novoPedido.DataHoraInclusao.AddHours(distanciaTotal / drone.Velocidade);

            return drone.Capacidade >= pesoTotal && drone.Perfomance >= distanciaTotal;


        }
    }
}
