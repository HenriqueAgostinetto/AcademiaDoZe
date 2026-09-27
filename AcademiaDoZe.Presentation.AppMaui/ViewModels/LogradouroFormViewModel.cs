// henrique agostinetto piva
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroFormViewModel(ILogradouroService service) : ObservableObject
{
    private readonly ILogradouroService _service = service;
    private int _id;

    [ObservableProperty] private string cep = string.Empty;
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string bairro = string.Empty;
    [ObservableProperty] private string cidade = string.Empty;
    [ObservableProperty] private string estado = string.Empty;
    [ObservableProperty] private string pais = "Brasil";

    public void Definir(LogradouroDto? item)
    {
        _id = item?.Id ?? 0;
        Cep = item?.Cep ?? string.Empty;
        Nome = item?.Nome ?? string.Empty;
        Bairro = item?.Bairro ?? string.Empty;
        Cidade = item?.Cidade ?? string.Empty;
        Estado = item?.Estado ?? string.Empty;
        Pais = item?.Pais ?? "Brasil";
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Cep) || string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Bairro) || string.IsNullOrWhiteSpace(Cidade) || string.IsNullOrWhiteSpace(Estado) || string.IsNullOrWhiteSpace(Pais))
        {
			await Shell.Current.DisplayAlertAsync("Atencao", "Preencha todos os campos.", "Ok");
            return;
        }
        try
        {
            var item = new LogradouroDto { Id = _id, Cep = Cep, Nome = Nome, Bairro = Bairro, Cidade = Cidade, Estado = Estado, Pais = Pais };
            if (_id == 0) await _service.AdicionarAsync(item); else await _service.AtualizarAsync(item);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception exception)
        {
			await Shell.Current.DisplayAlertAsync("Atencao", exception.Message, "Ok");
        }
    }

    [RelayCommand]
    private Task CancelarAsync() => Shell.Current.GoToAsync("..");
}
