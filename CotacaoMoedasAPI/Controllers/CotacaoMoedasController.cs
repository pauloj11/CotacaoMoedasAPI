using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class CotacaoMoedasController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;

    public CotacaoMoedasController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [HttpGet("conversao")]
    public async Task<IActionResult> ConverterMoeda([FromQuery] string moedaOrigem, [FromQuery] string moedaDestino, decimal valor)
    {
        try
        {
            // Construa a URL da API com as moedas de origem e destino na ordem correta
            string apiUrl = $"https://economia.awesomeapi.com.br/last/{moedaOrigem}-{moedaDestino}";

            // Realize uma chamada HTTP para obter as taxas de câmbio em tempo real da AwesomeAPI
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // Analise a resposta em JSON e realize a conversão
                var cotacaoJson = await response.Content.ReadAsStringAsync();
                var cotacao = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(cotacaoJson);
                decimal taxaDeCambio = Convert.ToDecimal(cotacao[$"{moedaOrigem}{moedaDestino}"].bid);
                decimal valorConvertido = valor * taxaDeCambio; // Correção aqui: multiplicar em vez de dividir

                return Ok(new
                {
                    ValorEmMoedaOrigem = valor,
                    MoedaOrigem = moedaOrigem,
                    ValorConvertido = valorConvertido,
                    MoedaDestino = moedaDestino
                });
            }
            else
            {
                return BadRequest("Falha ao obter as taxas de câmbio em tempo real.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

}
