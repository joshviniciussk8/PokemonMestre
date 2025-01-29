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
        private readonly ILogger<PokemonController> _logger;
        public PokemonCaptureController(IPokemonMasterRepository pokemonMasterRepository, HttpClient httpClient, IPasswordHasher<string> passwordHasher, ILogger<PokemonController> logger)
        {
            _pokemonMasterRepository = pokemonMasterRepository;
            _httpClient = httpClient;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }
        public async Task<int> Logar(string Login, string Senha)
        {
            _logger.LogInformation("Logando Mestre Pokémon com Login: {Login}", Login);
            try
            {
                PokemonMasterResponse pokemonMasterResponse = await _pokemonMasterRepository.GetPokemonMasterByUser(Login);
                if (pokemonMasterResponse == null)
                {
                    _logger.LogWarning("Mestre Pokémon Não encontrado: {@Login}", Login);
                    return 0;
                }
                var resultado = _passwordHasher.VerifyHashedPassword(Login, pokemonMasterResponse.Senha, Senha);
                if (resultado == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Mestre Pokémon Usuário ou senha inválida: {@Login}", Login);
                    return 0;
                }
                _logger.LogInformation("Mestre Pokémon Logado com Sucesso: {Login}", Login);
                return pokemonMasterResponse.Id;
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao buscar Pokémon com Usuário: {Login} verifique o Erro {ex}", Login, ex);
                return 0;
            }
            
        }
        [HttpGet("ListPokemonsByMestre")]
        [SwaggerOperation(Summary = "Lista Todos os Pokémons do Mestre")]
        public async Task<IEnumerable<PokemonCapture>> ListCapturePokemon(string Login, string Senha)
        {
            _logger.LogInformation("Buscando Lista de pokemons Capturados por Mestre Pokémon com Login: {Login}", Login);
            IEnumerable<PokemonCapture> PokemonCaptures = new List<PokemonCapture>();
            try
            {                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dados de Login Inválidos, Login: {@Login}", Login);
                    return PokemonCaptures;
                }
                   
                int MestreId = await Logar(Login, Senha);
                if (MestreId == 0)
                {
                    _logger.LogWarning("Dados de Login Inválidos, Login: {@Login}", Login);
                    return PokemonCaptures;
                }
                PokemonCaptures = await _pokemonMasterRepository.ListCapturePokemon(MestreId);
                _logger.LogInformation("Busca Lista de pokemons concluída: {Login} Pokemons: {PokemonCaptures}", Login, PokemonCaptures);
                return PokemonCaptures;
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao buscar Lista de pokemons com Login: {Login} verifique o Erro {ex}", Login, ex);
                return PokemonCaptures;
            }
            
        }
        [HttpPost("capture")]
        [SwaggerOperation(Summary = "Captura um Pokémon aleatório Para o Mestre")]
        public async Task<ActionResult<PokemonCapture>> CapturePokemon(string Login, string Senha)
        {
            _logger.LogInformation("Capturando Pokemon aleatório Login: {Login}", Login);
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Dados inválidos Login: {Login}", Login);
                    return BadRequest(ModelState);
                }                    
                int MestreId = await Logar(Login, Senha);
                if (MestreId == 0)
                {
                    _logger.LogWarning("Dados inválidos Login: {Login}", Login);
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
                if (resultado == 0)
                {
                    _logger.LogWarning("Houve um problema na captura Login: {Login} Pokemons {pokemonCapture}", Login, pokemonCapture);
                    return BadRequest();
                }
                _logger.LogInformation("Sucesso ao capturar Pokémon com Login: {Login} Pokemons: {pokemonCapture}", Login, pokemonCapture);
                return CreatedAtAction(nameof(CapturePokemon), new { id = pokemonCapture.Id }, pokemonCapture);
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao capturar Pokémon com Login: {Login} verifique o Erro {ex}", Login, ex);
                return BadRequest();
            }
            
        }
    }
}
