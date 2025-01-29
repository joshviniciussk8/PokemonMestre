using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http;
using System.Text.Json;

namespace Pokemon.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PokemonController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PokemonController> _logger;
        public PokemonController(HttpClient httpClient, ILogger<PokemonController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        [HttpGet("Pokemon/{id}")]
        [SwaggerOperation(Summary = "Traz um Pokemon pelo Id")]
        public async Task<IActionResult> GetPokemon(int id)
        {
            _logger.LogInformation("Buscando Pokémon com ID: {id}", id);
            var url = $"https://pokeapi.co/api/v2/pokemon/{id}";
            if (id > 1025)
            {
                _logger.LogWarning("Pokémon não encontrado: {@id}", id);
                return NotFound("Pokemon não encontrado!");
            }
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
                    _logger.LogInformation("Pokémon encontrado: {@pokemonData}", pokemonData);
                    return Ok(pokemonData);
                }
                _logger.LogWarning("Pokémon não encontrado: {@id}", id);
                return StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao buscar Pokémon com ID: {Pokemon}", id);
                return StatusCode(500, $"Erro ao conectar à API: {ex.Message}");
            }
        }
        [HttpGet("RandomPokemon")]
        [SwaggerOperation(Summary = "Traz 7 Pokémons aleatórios")]
        public async Task<IActionResult> GetRandomPokemons()
        {
            _logger.LogInformation("Buscando 7 Pokémons aleatórios");
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
                _logger.LogInformation("Pokémons encontrados: {@pokemons}", pokemons);
                return Ok(pokemons);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Erro ao buscar Pokémons: {Ex}", ex);
                return StatusCode(500, $"Erro ao conectar à API: {ex.Message}");
            }
        }
    }
}
