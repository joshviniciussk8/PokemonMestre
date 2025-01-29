using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http;
using System.Text.Json;

namespace Pokemon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PokemonController(HttpClient httpClient) : Controller
    {
        private readonly HttpClient _httpClient = httpClient;
        [HttpGet("Pokemon/{id}")]
        [SwaggerOperation(Summary = "Traz um Pokemon pelo Id")]
        public async Task<IActionResult> GetPokemon(int id)
        {
            var url = $"https://pokeapi.co/api/v2/pokemon/{id}";
            if (id > 1025) return NotFound("Pokemon não encontrado!");
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(content);

                    var pokemonData = new
                    {
                        id = jsonDocument.RootElement.GetProperty("id").GetInt32(),
                        name = jsonDocument.RootElement.GetProperty("name").GetString(),
                        sprite = jsonDocument.RootElement.GetProperty("sprites").GetProperty("front_default").GetString(),
                        types = jsonDocument.RootElement.GetProperty("types").EnumerateArray().Select(t => t.GetProperty("type").GetProperty("name").GetString()).ToList()
                    };

                    return Ok(pokemonData);
                }
                return StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException e)
            {
                return StatusCode(500, $"Erro ao conectar à API: {e.Message}");
            }
        }
        [HttpGet("RandomPokemon")]
        [SwaggerOperation(Summary = "Traz 7 Pokémons aleatórios")]
        public async Task<IActionResult> GetRandomPokemons()
        {
            var random = new Random();
            var randomIds = new int[7];

            for (int i = 0; i < 7; i++)
            {
                randomIds[i] = random.Next(1, 1025);
            }

            var pokemons = new List<object>();

            try
            {
                foreach (var id in randomIds)
                {
                    var url = $"https://pokeapi.co/api/v2/pokemon/{id}";
                    var response = await _httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var jsonDocument = JsonDocument.Parse(content);
                        var pokemonData = new
                        {
                            id = jsonDocument.RootElement.GetProperty("id").GetInt32(),
                            name = jsonDocument.RootElement.GetProperty("name").GetString(),
                            sprite = jsonDocument.RootElement.GetProperty("sprites").GetProperty("front_default").GetString(),
                            types = jsonDocument.RootElement.GetProperty("types").EnumerateArray().Select(t => t.GetProperty("type").GetProperty("name").GetString()).ToList()
                        };

                        pokemons.Add(pokemonData);
                    }
                }
                return Ok(pokemons);
            }
            catch (HttpRequestException e)
            {
                return StatusCode(500, $"Erro ao conectar à API: {e.Message}");
            }
        }
    }
}
