using Pokemon.Models;
namespace Pokemon.Repositories.Interfaces
{
    public interface IPokemonMasterRepository
    {
        Task<PokemonMasterResponse> GetPokemonMasterById(int id);
        Task<PokemonMasterResponse> GetPokemonMasterByUser(string User);
        Task<PokemonMasterResponse> LoginMaster(string user, string password);
        Task<int> AddPokemonMaster(PokemonMasterRequest pokemonMaster);
        Task<IEnumerable<PokemonMasterResponse>> GetAllPokemonMasters();
        Task<int> CapturePokemonRandom(PokemonCapture pokemonCapture);
        Task<IEnumerable<PokemonCapture>> ListCapturePokemon(int MasterId);
        Task<bool> DeleteMasterPokemon(int MasterId);
        Task<bool> EditMasterPokemon(PokemonMasterRequest pokemonMasterRequest);
    }
}
