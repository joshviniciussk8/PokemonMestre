using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Pokemon.Models;
using Pokemon.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace Pokemon.Repositories
{
    public class PokemonCaptureRepository : IPokemonMasterRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<PokemonCaptureRepository> _logger;
        public PokemonCaptureRepository(IDbConnection dbConnection, ILogger<PokemonCaptureRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }
        public async Task<int> AddPokemonMaster(PokemonMasterRequest pokemonMaster)
        {
            string sql = "INSERT INTO Mestre (Nome, Usuario, Senha) values (@Nome, @Usuario, @Senha); SELECT last_insert_rowid();";
            try
            {
                return await _dbConnection.ExecuteScalarAsync<int>(sql, pokemonMaster);                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao Adicionar Pokémon ao Mestre {Pokemon}", pokemonMaster);
                return 0;
            }
        }
        public async Task<int> CapturePokemonRandom(PokemonCapture pokemonCapture)
        {
            try
            {
                string sql = "INSERT INTO PokemonCapture (PokemonName, PokemonId, PokemonSprite, PokemonTypes, PokemonMasterId) values (@PokemonName, @PokemonId, @PokemonSprite, @PokemonTypes, @PokemonMasterId); SELECT last_insert_rowid();";
                return await _dbConnection.ExecuteScalarAsync<int>(sql, pokemonCapture);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao Capturar Pokémon {Pokemon}", pokemonCapture);
                return 0;
            }
        }
        public async Task<bool> DeleteMasterPokemon(int MasterId)
        {
            try
            {
                string sql = "Delete From Mestre Where Id  = @id";                
                var parametros = new { id = MasterId };
                int rowsAffected = await _dbConnection.ExecuteAsync(sql, parametros);                
                return rowsAffected>0;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao Deletar Mestre Pokémon ID {Id}",MasterId);
                return false;
            }
        }
        public async Task<bool> EditMasterPokemon(PokemonMasterRequest pokemonMasterRequest)
        {
            try
            {
                string sql = "update Mestre set Nome = @Nome, Usuario= @Usuario, Senha = @Senha where Id = @Id";
                int rowsAffected = await _dbConnection.ExecuteAsync(sql, pokemonMasterRequest);
                return rowsAffected > 0;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao Editar Mestre Pokémon.");
                return false;
            }
        }
        public async Task<IEnumerable<PokemonMasterResponse>> GetAllPokemonMasters()
        {
            try
            {
                string sql = "SELECT * FROM Mestre;";
                return await _dbConnection.QueryAsync<PokemonMasterResponse>(sql);         
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Erro ao buscar Mestres Pokémon");
                return Enumerable.Empty<PokemonMasterResponse>();
            }
        }
        public async Task<PokemonMasterResponse?> GetPokemonMasterById(int id)
        {
            try
            {
                string sql = "SELECT * FROM Mestre WHERE Id = @Id;";
                var parametros = new { Id = id };
                return await _dbConnection.QuerySingleOrDefaultAsync<PokemonMasterResponse>(sql, parametros);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Mestres com o ID {Id}",id);
                return null;
            }
        }
        public async Task<PokemonMasterResponse?> GetPokemonMasterByUser(string User)
        { 
            try
            {
                string sql = "SELECT * FROM Mestre WHERE Usuario = @Usuario;";
                var parametros = new { Usuario = User };
                return await _dbConnection.QuerySingleOrDefaultAsync<PokemonMasterResponse>(sql, parametros);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Mestres com o Usuario {Us}", User);
                return null;
            }
        }
        public async Task<IEnumerable<PokemonCapture>> ListCapturePokemon(int masterId)
        {
            try
            {
                string sql = "select * from PokemonCapture where PokemonMasterId = @MasterId;";
                var parametros = new { MasterId = masterId };
                return await _dbConnection.QueryAsync<PokemonCapture>(sql, parametros);
                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao ListarPokemons do Mestres com o ID {Id}", masterId);
                return Enumerable.Empty<PokemonCapture>();
            }
        }
        public async Task<PokemonMasterResponse?> LoginMaster([FromBody]string user, string password)
        {
            try
            {
                string sql = "SELECT * FROM Mestre WHERE Usuario = @Usuario and Senha = @Senha;";
                var parametros = new { Usuario = user, Senha = password };
                return await _dbConnection.QuerySingleOrDefaultAsync<PokemonMasterResponse>(sql, parametros);                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao Logar Mestres com o Usuário {User}", user);
                return null;
            }
        }
    }
}
