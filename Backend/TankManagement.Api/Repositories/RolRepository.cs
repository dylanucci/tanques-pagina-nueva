using Microsoft.EntityFrameworkCore;
using TankManagement.Api.Data;
using TankManagement.Api.Interfaces;
using TankManagement.Api.Models;
namespace TankManagement.Api.Repositories;
public class RolRepository(TankManagementDbContext db) : IRolRepository
{
    public Task<Rol?> ObtenerPorIdAsync(int id) => db.Roles.FindAsync(id).AsTask();
}
