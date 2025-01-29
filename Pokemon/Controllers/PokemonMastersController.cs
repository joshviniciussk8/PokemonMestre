using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using Pokemon.Models;

using Pokemon.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Logging;

namespace Pokemon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PokemonMastersController : Controller
    {
        private readonly IPasswordHasher<PokemonMasterRequest> _passwordHasher;
        private readonly IPokemonMasterRepository _pokemonMasterRepository;
        private readonly ILogger<PokemonController> _logger;

        public PokemonMastersController(IPokemonMasterRepository iPokemonMasterRepository, IPasswordHasher<PokemonMasterRequest> passwordHasher, ILogger<PokemonController> logger)
        {
            _pokemonMasterRepository = iPokemonMasterRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }
        [HttpPost("Masters")]
        [SwaggerOperation(Summary = "Cria um novo Mestre")]
        public async Task<ActionResult<PokemonMasterResponse>> PostPokemonMaster([FromBody] PokemonMasterRequest pokemonMaster)
        {
            _logger.LogInformation("Criando Mestre Pokémon: {pokemonMaster}", pokemonMaster);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Tem algo na validação de Dados: {@pokemonMaster}", pokemonMaster);
                return BadRequest(ModelState);
            }
                
            try
            {
                var existingMaster = await _pokemonMasterRepository.GetPokemonMasterByUser(pokemonMaster.Usuario);
                if (existingMaster != null)
                {
                    _logger.LogWarning("Mestre Pokémon Já existe: {@pokemonMaster}", pokemonMaster);
                    return BadRequest("Já existe um mestre Pokémon com este Usuário.");
                }
                pokemonMaster.Senha = _passwordHasher.HashPassword(pokemonMaster, pokemonMaster.Senha);
                int response = await _pokemonMasterRepository.AddPokemonMaster(pokemonMaster);
                if (response > 0)
                {
                    _logger.LogInformation("Mestre Pokémon Criado: {pokemonMaster}", pokemonMaster);
                    return CreatedAtAction(nameof(GetPokemonMaster), new { id = pokemonMaster.Usuario }, pokemonMaster);
                }
                else
                {
                    _logger.LogWarning("Houve algum Problema na interface de criação: {@pokemonMaster}", pokemonMaster);
                    return BadRequest("Ops Algo deu errado!");
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Erro ao Cadastrar Mestre Pokémon verifique o erro {ex}: {@pokemonMaster}", ex, pokemonMaster);
                return BadRequest("Ops Algo deu errado!");
            }
            
        }

        [HttpGet("Master/{usuario}")]
        [SwaggerOperation(Summary = "Traz os dados de um Mestre através do usuário")]
        public async Task<ActionResult<PokemonMasterResponse>> GetPokemonMaster(string usuario)
        {
            _logger.LogInformation("Buscando Mastre Pokémon com Usuário: {usuario}", usuario);
            try 
            {
                var pokemonMaster = await _pokemonMasterRepository.GetPokemonMasterByUser(usuario);
                if (pokemonMaster == null)
                {
                    _logger.LogWarning("Mestre Pokémon não encontrado com Usuário: {usuario}", usuario);
                    return NotFound();
                }
                return pokemonMaster;
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao buscar Mestre Pokémon com o usuario: {usuario} {ex}", usuario, ex);
                return BadRequest("Ops Algo deu errado!");
            }
            
        }
        [HttpGet("MasterById/{id}")]
        [SwaggerOperation(Summary = "Traz os dados de um Mestre Por ID")]
        public async Task<ActionResult<PokemonMasterResponse>> GetPokemonMaster(int id)
        {
            _logger.LogInformation("Buscando Mestre Pokémon com ID: {id}", id);
            try
            {
                _logger.LogWarning("Mestre Pokémon não encontrado com ID: {id}", id);
                var pokemonMaster = await _pokemonMasterRepository.GetPokemonMasterById(id);
                if (pokemonMaster == null)
                {
                    return NotFound();
                }
                return pokemonMaster;
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao buscar Mestre Pokémon com ID: {id} verifique o Erro {ex}", id, ex);
                return BadRequest("Ops Algo deu errado!");
            }
            
        }
        [HttpGet("AllMasters")]
        [SwaggerOperation(Summary = "Traz todos os Mestre da Plataforma")]
        public async Task<IEnumerable<PokemonMasterResponse>> GetAllPokemonMasters()
        {
            _logger.LogInformation("Buscando Todos os Mestres Pokémon:");
            IEnumerable<PokemonMasterResponse>? pokemonMasters = null;
            try
            {
                pokemonMasters = await _pokemonMasterRepository.GetAllPokemonMasters();
                _logger.LogInformation(" Todos os Mestres Pokémons Encontrados {pokemonMasters}:", pokemonMasters);
                return  pokemonMasters;
                
            }
            catch(Exception ex)
            {
                _logger.LogError("Erro ao buscar Todos os Mestres Pokémons: {pokemonMasters} verifique o Erro {ex}", pokemonMasters, ex);
                return pokemonMasters;
            }
            
        }
        [HttpDelete("MasterDelete/{id}")]
        [SwaggerOperation(Summary = "Deleta um Mestre Pokémon pelo Id")]
        public async Task<IActionResult> DeletePokemonMasters(int id) {
            _logger.LogInformation("Deletando Mestre Pokémon com ID: {id}", id);

            try
            {
                var ValidaMaster = await GetPokemonMaster(id);
                if (ValidaMaster.Value == null)
                {
                    _logger.LogWarning("Mestre Pokemon não encontrado com o ID: {@id}", id);
                    return NotFound(new { message = "Registro não encontrado." });
                }
                var response = await _pokemonMasterRepository.DeleteMasterPokemon(id);
                if (response)
                {
                    _logger.LogInformation("Mestre Pokemon Deletado com Sucesso com o ID: {@id}", id);
                    return Ok(new { message = "Registro deletado com sucesso!" });
                }
                else 
                {
                    _logger.LogWarning("Mestre Pokemon não encontrado com o ID: {@id}", id);
                    return NotFound(new { message = "Registro não encontrado." });
                } 
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao Deletar Mestre Pokémon com ID: {id} verifique o Erro {ex}", id, ex);
                return BadRequest($"Ops Algo deu errado! Verifique o Erro {ex}");
            }

        }
        [HttpPut("EditMaster")]
        [SwaggerOperation(Summary = "Edita os dados do Mestre")]
        public async Task<IActionResult> EditPokemonMaster(int id, [FromBody] PokemonMasterRequest MasterNovo)
        {
            _logger.LogInformation("Editando Mestre Pokémon com ID: {id}", id);
            try
            {
                var MasterExistente = await GetPokemonMaster(id);
                if (MasterExistente.Value == null)
                {
                    _logger.LogWarning("Mestre Pokemon não encontrado com o ID: {@id}", id);
                    return NotFound(new { message = "Registro não encontrado." });
                }

                if (MasterNovo.Senha != null)
                {
                    var verificaSenhaHash = _passwordHasher.VerifyHashedPassword(MasterNovo, MasterExistente.Value.Senha, MasterNovo.Senha);
                    MasterNovo.Senha = _passwordHasher.HashPassword(MasterNovo, MasterNovo.Senha);
                }
                else
                {
                    MasterNovo.Senha = MasterExistente.Value.Senha;
                }                
                if (MasterNovo.Usuario == null)
                {
                    MasterNovo.Usuario = MasterExistente.Value.Usuario;
                }
                if (MasterNovo.Nome == null)
                {
                    MasterNovo.Nome = MasterExistente.Value.Nome;
                }
                MasterNovo.Id = id;
                await _pokemonMasterRepository.EditMasterPokemon(MasterNovo);
                _logger.LogInformation("Mestre Pokemon Editado com sucesso ID: {@id} Mestre: {MasterExistente}", id, MasterExistente);
                return Ok(new { message = "Mestre Pokémon Editado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao Editar Mestre Pokémon com ID: {id} verifique o Erro {ex}", id, ex);
                return BadRequest($"Ops Algo deu errado! Verifique o Erro {ex}");
            }
            
        }
    }
}
