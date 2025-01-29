using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using Pokemon.Models;

using Pokemon.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

namespace Pokemon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PokemonMastersController : Controller
    {
        private readonly IPasswordHasher<PokemonMasterRequest> _passwordHasher;
        private readonly IPokemonMasterRepository _pokemonMasterRepository;
        public PokemonMastersController(IPokemonMasterRepository iPokemonMasterRepository, IPasswordHasher<PokemonMasterRequest> passwordHasher)
        {
            _pokemonMasterRepository = iPokemonMasterRepository;
            _passwordHasher = passwordHasher;
        }
        [HttpPost("Masters")]
        [SwaggerOperation(Summary = "Cria um novo Mestre")]
        public async Task<ActionResult<PokemonMasterResponse>> PostPokemonMaster([FromBody] PokemonMasterRequest pokemonMaster)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var existingMaster = await _pokemonMasterRepository.GetPokemonMasterByUser(pokemonMaster.Usuario);
                if (existingMaster != null)
                {
                    return BadRequest("Já existe um mestre Pokémon com este Usuário.");
                }
                pokemonMaster.Senha = _passwordHasher.HashPassword(pokemonMaster, pokemonMaster.Senha);
                int response = await _pokemonMasterRepository.AddPokemonMaster(pokemonMaster);
                if (response > 0)
                {
                    return CreatedAtAction(nameof(GetPokemonMaster), new { id = pokemonMaster.Usuario }, pokemonMaster);
                }
                else
                {
                    return BadRequest("Ops Algo deu errado!");
                }
            }
            catch
            {
                return BadRequest("Ops Algo deu errado!");
            }
            
        }

        [HttpGet("Master/{usuario}")]
        [SwaggerOperation(Summary = "Traz os dados de um Mestre através do usuário")]
        public async Task<ActionResult<PokemonMasterResponse>> GetPokemonMaster(string usuario)
        {
            try
            {
                var pokemonMaster = await _pokemonMasterRepository.GetPokemonMasterByUser(usuario);
                if (pokemonMaster == null)
                {
                    return NotFound();
                }
                return pokemonMaster;
            }
            catch
            {
                return BadRequest("Ops Algo deu errado!");
            }
            
        }
        [HttpGet("MasterById/{id}")]
        [SwaggerOperation(Summary = "Traz os dados de um Mestre Por ID")]
        public async Task<ActionResult<PokemonMasterResponse>> GetPokemonMaster(int id)
        {
            try
            {
                var pokemonMaster = await _pokemonMasterRepository.GetPokemonMasterById(id);
                if (pokemonMaster == null)
                {
                    return NotFound();
                }
                return pokemonMaster;
            }
            catch
            {
                return BadRequest("Ops Algo deu errado!");
            }
            
        }
        [HttpGet("AllMasters")]
        [SwaggerOperation(Summary = "Traz todos os Mestre da Plataforma")]
        public async Task<IEnumerable<PokemonMasterResponse>> GetAllPokemonMasters()
        {
            IEnumerable<PokemonMasterResponse>? pokemonMasters = null;
            try
            {
                pokemonMasters = await _pokemonMasterRepository.GetAllPokemonMasters();
                return  pokemonMasters;
                
            }
            catch
            {
                return pokemonMasters;
            }
            
        }
        [HttpDelete("MasterDelete/{id}")]
        [SwaggerOperation(Summary = "Deleta um Mestre Pokémon pelo Id")]
        public async Task<IActionResult> DeletePokemonMasters(int id) {
            try
            {
                var ValidaMaster = await GetPokemonMaster(id);
                if (ValidaMaster.Value == null)
                {
                    return NotFound(new { message = "Registro não encontrado." });
                }
                var response = await _pokemonMasterRepository.DeleteMasterPokemon(id);
                if (response) return Ok(new { message = "Registro deletado com sucesso!" });
                else return NotFound(new { message = "Registro não encontrado." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Ops Algo deu errado! Verifique o Erro {ex}");
            }

        }
        [HttpPut("EditMaster")]
        [SwaggerOperation(Summary = "Edita os dados do Mestre")]
        public async Task<IActionResult> EditPokemonMaster(int id, [FromBody] PokemonMasterRequest MasterNovo)
        {
            try
            {
                var MasterExistente = await GetPokemonMaster(id);
                if (MasterExistente.Value == null)
                {
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
                return Ok(new { message = "Mestre Pokémon Editado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Ops Algo deu errado! Verifique o Erro {ex}");
            }
            
        }
    }
}
