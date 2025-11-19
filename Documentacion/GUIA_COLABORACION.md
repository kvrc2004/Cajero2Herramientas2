# 👥 Guía de Colaboración - Proyecto MiBanco

## 🎯 Para Nuevos Colaboradores

Si estás uniéndote al proyecto por primera vez, sigue estos pasos:

### 1. Clonar el Repositorio

```bash
git clone https://github.com/kvrc2004/Cajero2Herramientas2.git
cd Cajero2Herramientas2
```

### 2. Instalar la Base de Datos

**📖 Ver guía completa:** [INSTRUCCIONES_INSTALACION.md](Database/INSTRUCCIONES_INSTALACION.md)

**Pasos rápidos:**
1. Abre SQL Server Management Studio
2. Ejecuta el script: `Database/CreateDatabase_MiPlata.sql`
3. Verifica que se creó la base de datos `MiPlataDB`

### 3. Configurar tu Cadena de Conexión LOCAL

⚠️ **IMPORTANTE**: Cada desarrollador tiene su propia configuración de SQL Server

**Edita `appsettings.json` según tu entorno:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=MiPlataDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Ejemplos de cadenas de conexión:**

```json
// SQL Express (común en laptops)
"Server=localhost\\SQLEXPRESS;Database=MiPlataDB;Trusted_Connection=True;TrustServerCertificate=True;"

// SQL Server Local (instancia completa)
"Server=localhost;Database=MiPlataDB;Trusted_Connection=True;TrustServerCertificate=True;"

// Con autenticación SQL
"Server=localhost;Database=MiPlataDB;User Id=tuUsuario;Password=tuPassword;TrustServerCertificate=True;"

// Servidor remoto o de desarrollo
"Server=192.168.1.100;Database=MiPlataDB;User Id=dev;Password=dev123;TrustServerCertificate=True;"
```

### 4. Restaurar Paquetes y Ejecutar

```bash
dotnet restore
dotnet build
dotnet run
```

### 5. Probar la Aplicación

- Abre el navegador en: https://localhost:5001
- Usuario de prueba: `juan.perez`
- Contraseña: `123456`

---

## 🔄 Workflow de Desarrollo

### Antes de Empezar a Trabajar

```bash
# Asegúrate de estar en la rama main
git checkout main

# Descarga los últimos cambios
git pull origin main

# Crea una nueva rama para tu trabajo
git checkout -b feature/nombre-de-tu-funcionalidad
```

### Durante el Desarrollo

1. **Trabaja en tu rama**: Nunca trabajes directamente en `main`
2. **Commits frecuentes**: Haz commits pequeños y descriptivos
3. **Prueba antes de commitear**: Asegúrate de que todo funciona

```bash
# Ver cambios
git status

# Agregar archivos
git add .

# Commit con mensaje descriptivo
git commit -m "feat: descripción clara de lo que hiciste"
```

### Tipos de Commits (Convención)

- `feat:` Nueva funcionalidad
- `fix:` Corrección de bug
- `docs:` Cambios en documentación
- `style:` Formato, punto y coma, etc.
- `refactor:` Reestructuración de código
- `test:` Agregar tests
- `chore:` Tareas de mantenimiento

**Ejemplos:**
```bash
git commit -m "feat: añadir validación de monto en transferencias"
git commit -m "fix: corregir cálculo de intereses en cuenta de ahorros"
git commit -m "docs: actualizar README con instrucciones de BD"
```

### Subir tus Cambios

```bash
# Subir tu rama al repositorio
git push origin feature/nombre-de-tu-funcionalidad
```

### Crear Pull Request

1. Ve a GitHub: https://github.com/kvrc2004/Cajero2Herramientas2
2. Haz clic en "Compare & pull request"
3. Describe tus cambios claramente
4. Espera la revisión de tus compañeros
5. Una vez aprobado, se fusionará a `main`

---

## 🚫 Archivos que NO debes Modificar (Sin Coordinación)

- ❌ `Database/CreateDatabase_MiPlata.sql` - Solo modificar si todos están de acuerdo
- ❌ `appsettings.json` - Solo cambios estructurales, NO tu cadena de conexión
- ⚠️ `Models/` - Avisar al equipo antes de cambiar modelos
- ⚠️ `Data/MiBancoDbContext.cs` - Coordinar cambios en el DbContext

---

## ✅ Mejores Prácticas

### 1. Configuración Personal vs Configuración del Proyecto

**❌ NO HACER:**
```json
// NO commites tu cadena de conexión personal
"Server=LAPTOP-JUAN\\SQLEXPRESS;..."
```

**✅ HACER:**
```json
// Usa la configuración genérica en el repo
"Server=localhost;Database=MiPlataDB;Trusted_Connection=True;..."
```

### 2. Base de Datos

- ✅ El script SQL debe funcionar en CUALQUIER instalación de SQL Server
- ✅ No uses rutas absolutas para archivos de BD
- ✅ Usa nombres de servidor genéricos (`localhost`)

### 3. Código

- ✅ Comenta tu código en español
- ✅ Usa nombres descriptivos para variables y métodos
- ✅ Sigue las convenciones de C# (PascalCase para métodos, camelCase para variables)
- ✅ Prueba tu código antes de hacer push

### 4. Comunicación

- 💬 Usa issues de GitHub para reportar bugs
- 💬 Comenta en los Pull Requests
- 💬 Documenta cambios importantes
- 💬 Avisa al equipo sobre cambios en la BD

---

## 🆘 Solución de Problemas Comunes

### "No puedo conectar a la base de datos"

**Solución:**
1. Verifica que SQL Server esté corriendo
2. Ajusta tu cadena de conexión en `appsettings.json`
3. Ejecuta el script SQL nuevamente si es necesario

```bash
# Probar conexión desde CMD
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

### "Tengo conflictos en Git"

**Solución:**
```bash
# Guardar tus cambios temporalmente
git stash

# Actualizar desde main
git pull origin main

# Recuperar tus cambios
git stash pop

# Resolver conflictos manualmente
# Luego:
git add .
git commit -m "merge: resolver conflictos con main"
```

### "La aplicación no compila después de hacer pull"

**Solución:**
```bash
# Limpiar y restaurar
dotnet clean
dotnet restore
dotnet build
```

### "Mi base de datos tiene datos viejos"

**Solución:**
```sql
-- En SSMS, ejecutar:
USE master;
GO

ALTER DATABASE MiPlataDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE MiPlataDB;
GO

-- Luego ejecutar CreateDatabase_MiPlata.sql nuevamente
```

---

## 📋 Checklist para Cada Sesión de Trabajo

### Al Iniciar
- [ ] `git pull origin main` - Descargar últimos cambios
- [ ] `git checkout -b feature/mi-rama` - Crear rama de trabajo
- [ ] Verificar que SQL Server está corriendo
- [ ] Verificar que la aplicación compila: `dotnet build`

### Durante el Trabajo
- [ ] Commits frecuentes con mensajes descriptivos
- [ ] Probar cambios localmente
- [ ] Documentar funcionalidades nuevas

### Al Terminar
- [ ] `git push origin mi-rama` - Subir cambios
- [ ] Crear Pull Request en GitHub
- [ ] Notificar al equipo para revisión

---

## 🎓 Recursos Útiles

### Git
- [Git Cheat Sheet](https://education.github.com/git-cheat-sheet-education.pdf)
- [Atlassian Git Tutorial](https://www.atlassian.com/git/tutorials)

### C# / ASP.NET Core
- [Documentación oficial de .NET](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

### SQL Server
- [SQL Server Documentation](https://docs.microsoft.com/sql/sql-server/)
- [T-SQL Reference](https://docs.microsoft.com/sql/t-sql/)

---

## 👨‍💻 Equipo de Desarrollo

Si tienes dudas, consulta con el equipo:

- **Problemas de Git**: Ayuda mutua en el grupo
- **Problemas de BD**: Revisar [INSTRUCCIONES_INSTALACION.md](Database/INSTRUCCIONES_INSTALACION.md)
- **Bugs**: Crear issue en GitHub

---

## 📞 Contacto y Soporte

Para problemas técnicos:
1. Busca en los archivos de documentación
2. Revisa los issues existentes en GitHub
3. Crea un nuevo issue describiendo el problema
4. Comparte el error exacto y pasos para reproducirlo

---

**¡Bienvenido al equipo! 🎉**

*Recuerda: El código es compartido, pero tu entorno de desarrollo es personal. Configura tu `appsettings.json` según tu máquina y no lo commites si solo cambias la cadena de conexión.*
