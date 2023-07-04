using Microsoft.AspNetCore.Mvc;
using PIK.FileStorage.Models;

using IOFile = System.IO.File;

namespace PIK.FileStorage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
	private readonly string _fileStorageDirectory;


	public FileController(IConfiguration configuration)
	{
		_fileStorageDirectory = configuration["FileStoragePath"];
	}


	[HttpGet("{fileName}")]
	public async Task<ActionResult> Download(string fileName)
	{
		string filePath = Path.Combine(_fileStorageDirectory, fileName);
		if (!IOFile.Exists(filePath))
			return NotFound();

		byte[] fileContent = await IOFile.ReadAllBytesAsync(filePath);
		return File(fileContent, "application/octet-stream", fileName);
	}

	[HttpPost]
	public async Task<ActionResult<string>> Upload([FromForm] UploadFileDto uploadFileDto)
	{
		try
		{
			CreateStorageDirectory();

			IFormFile formFile = uploadFileDto.File;
			string filePath = Path.Combine(_fileStorageDirectory, formFile.FileName);
			await using var fileStream = new FileStream(filePath, FileMode.Create);

			fileStream.Position = 0;
			await formFile.CopyToAsync(fileStream);

			return Ok(formFile.FileName);
		}
		catch
		{
			return StatusCode(
				StatusCodes.Status500InternalServerError,
				$"Не удалось загрузить файл \"{uploadFileDto.File.FileName}\".");
		}
	}


	private void CreateStorageDirectory() => Directory.CreateDirectory(_fileStorageDirectory);
}