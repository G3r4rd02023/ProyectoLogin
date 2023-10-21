namespace ProyectoLogin.Services
{
    public interface IFilesService
    {
        Task<string> SubirArchivo(Stream archivo, string nombre);
    }
}
