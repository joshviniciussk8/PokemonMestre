using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Pokemon.Models;
using Pokemon.Repositories.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net.Http;

namespace Pokemon.Controllers
{
    public class PokemonCaptureController : Controller
    {
        private readonly IPokemonMasterRepository _pokemonMasterRepository;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly HttpClient _httpClient;
        public PokemonCaptureController(IPokemonMasterRepository pokemonMasterRepository, HttpClient httpClient, IPasswordHasher<string> passwordHasher)
        {
            _pokemonMasterRepository = pokemonMasterRepository;
            _httpClient = httpClient;
            _passwordHasher = passwordHasher;
        }
        public async Task<int> Logar(string Login, string Senha)
        {
            PokemonMasterResponse pokemonMasterResponse = await _pokemonMasterRepository.GetPokemonMasterByUser(Login);
            if (pokemonMasterResponse == null)
            {
                return 0;
            }
            var resultado = _passwordHasher.VerifyHashedPassword(Login, pokemonMasterResponse.Senha, Senha);
            if (resultado == PasswordVerificationResult.Failed)
            {
                return 0;
            }
            return pokemonMasterResponse.Id;
        }
        [HttpGet("ListPokemonsByMestre")]
        [SwaggerOperation(Summary = "Lista Todos os Pokémons do Mestre")]
        public async Task<IEnumerable<PokemonCapture>> ListCapturePokemon(string Login, string Senha)
        {
            IEnumerable<PokemonCapture> PokemonCaptures = new List<PokemonCapture>();
            if (!ModelState.IsValid)
                return PokemonCaptures;
            int MestreId = await Logar(Login, Senha);
            if (MestreId == 0)
            {
                return PokemonCaptures;
            }
            PokemonCaptures = await _pokemonMasterRepository.ListCapturePokemon(MestreId);
            return PokemonCaptures;
        }
        [HttpPost("capture")]
        [SwaggerOperation(Summary = "Captura um Pokémon aleatório Para o Mestre")]
        public async Task<ActionResult<PokemonCapture>> CapturePokemon(string Login, string Senha)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            int MestreId = await Logar(Login, Senha);
            if (MestreId == 0)
            {
                return Unauthorized("Usuário ou Senhas Inválidos");
            }

            var random = new Random();
            int randomPokemonId = random.Next(1, 1025); 
            var response = await _httpClient.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{randomPokemonId}");
            var pokemonData = JObject.Parse(response);
            var pokemonName = pokemonData["name"].ToString();
            var pokemonSprite = pokemonData["sprites"]["front_default"].ToString();
            var typesArray = pokemonData["types"].Select(t => t["type"]["name"].ToString()).ToArray();
            var pokemonTypes = string.Join(", ", typesArray);
            var pokemonCapture = new PokemonCapture
            {
                PokemonName = pokemonName,
                PokemonId = randomPokemonId,
                PokemonSprite = pokemonSprite,
                PokemonTypes = pokemonTypes,
                PokemonMasterId = MestreId
            };
            int resultado = await _pokemonMasterRepository.CapturePokemonRandom(pokemonCapture);
            if(resultado == 0)
            {
                return BadRequest();
            }
            return CreatedAtAction(nameof(CapturePokemon), new { id = pokemonCapture.Id }, pokemonCapture);
        }
    }
}
