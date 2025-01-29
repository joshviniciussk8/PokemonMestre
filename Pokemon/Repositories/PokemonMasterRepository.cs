using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Pokemon.Models;
using Pokemon.Repositories.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Pokemon.Repositories
{
    public class PokemonCaptureRepository : IPokemonMasterRepository
    {
        private readonly IDbConnection _dbConnection;
        public PokemonCaptureRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<int> AddPokemonMaster(PokemonMasterRequest pokemonMaster)
        {
            int response; 
            string sql = "INSERT INTO Mestre (Nome, Usuario, Senha) values (@Nome, @Usuario, @Senha); SELECT last_insert_rowid();";
            try
            {
                _dbConnection.Open();
                response = await _dbConnection.ExecuteScalarAsync<int>(sql, pokemonMaster);
                _dbConnection.Close();
                return response;
            }
            catch 
            {
                return 0;
            }
        }
        public async Task<int> CapturePokemonRandom(PokemonCapture pokemonCapture)
        {
            string sql = "INSERT INTO PokemonCapture (PokemonName, PokemonId, PokemonSprite, PokemonTypes, PokemonMasterId) values (@PokemonName, @PokemonId, @PokemonSprite, @PokemonTypes, @PokemonMasterId); SELECT last_insert_rowid();";
            try
            {
                _dbConnection.Open();
                 var response = await _dbConnection.ExecuteScalarAsync<int>(sql, pokemonCapture);
                _dbConnection.Close();
                return response;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<bool> DeleteMasterPokemon(int MasterId)
        {
            try
            {
                string sql = "Delete From Mestre Where Id  = @id";
                _dbConnection.Open();
                var parametros = new { id = MasterId };
                await _dbConnection.ExecuteScalarAsync<PokemonMasterResponse>(sql, parametros);
                _dbConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> EditMasterPokemon(PokemonMasterRequest pokemonMasterRequest)
        {
            try
            {
                _dbConnection.Open();
                string sql = "update Mestre set Nome = @Nome, Usuario= @Usuario, Senha = @Senha where Id = @Id";
                await _dbConnection.ExecuteAsync(sql, pokemonMasterRequest);
                _dbConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<PokemonMasterResponse>> GetAllPokemonMasters()
        {
            IEnumerable<PokemonMasterResponse>? pokemonMasterResponse = null;
            try
            {
                _dbConnection.Open();
                string sql = "SELECT * FROM Mestre;";
                pokemonMasterResponse = await _dbConnection.QueryAsync<PokemonMasterResponse>(sql);
                _dbConnection.Close();
                return pokemonMasterResponse;
            }
            catch
            {
                return pokemonMasterResponse;
            }
        }
        public async Task<PokemonMasterResponse> GetPokemonMasterById(int id)
        {
            PokemonMasterResponse? pokemonMasterResponse = null;
            try
            {
                _dbConnection.Open();
                string sql = "SELECT * FROM Mestre WHERE Id = @Id;";
                var parametros = new { Id = id };
                pokemonMasterResponse = await _dbConnection.QuerySingleOrDefaultAsync<PokemonMasterResponse>(sql, parametros);
                _dbConnection.Close();
                return pokemonMasterResponse;

            }
            catch
            {
                return pokemonMasterResponse;
            }
        }
        public async Task<PokemonMasterResponse> GetPokemonMasterByUser(string User)
        { 
            PokemonMasterResponse? pokemonMasterResponse = null;
            try
            {
                _dbConnection.Open();
                string sql = "SELECT * FROM Mestre WHERE Usuario = @Usuario;";
                var parametros = new { Usuario = User };
                pokemonMasterResponse = await _dbConnection.QuerySingleOrDefaultAsync<PokemonMasterResponse>(sql, parametros);
                _dbConnection.Close();
                return pokemonMasterResponse;
                
            }
            catch
            {
                return pokemonMasterResponse;
            }
        }
        public async Task<IEnumerable<PokemonCapture>> ListCapturePokemon(int masterId)
        {
            IEnumerable<PokemonCapture>? PokemonCaptures = null;
            try
            {
                _dbConnection.Open();
                string sql = "select * from PokemonCapture where PokemonMasterId = @MasterId;";
                var parametros = new { MasterId = masterId };
                PokemonCaptures = await _dbConnection.QueryAsync<PokemonCapture>(sql, parametros);
                _dbConnection.Close();
                return PokemonCaptures;
            }
            catch
            {
                return PokemonCaptures;
            }
        }
        public async Task<PokemonMasterResponse> LoginMaster([FromBody]string user, string password)
        {
            PokemonMasterResponse? pokemonMasterResponse = null;
            try
            {
                _dbConnection.Open();
                string sql = "SELECT * FROM Mestre WHERE Usuario = @Usuario and Senha = @Senha;";
                var parametros = new { Usuario = user, Senha = password };
                pokemonMasterResponse = await _dbConnection.QuerySingleOrDefaultAsync<PokemonMasterResponse>(sql, parametros);
                _dbConnection.Close();
                return pokemonMasterResponse;

            }
            catch
            {
                return pokemonMasterResponse;
            }
        }
    }
}
