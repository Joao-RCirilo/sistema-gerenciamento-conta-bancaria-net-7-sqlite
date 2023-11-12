using System;
using Microsoft.Data.Sqlite;


class Program
{
    static void Main()
    {
        Banco banco = new Banco();

        while (true)
        {
            Console.WriteLine("1. Criar Conta");
            Console.WriteLine("2. Depositar Dinheiro");
            Console.WriteLine("3. Sacar Dinheiro");
            Console.WriteLine("4. Verificar Saldo");
            Console.WriteLine("5. Listar Contas");
            Console.WriteLine("6. Sair da Conta");

            int opcao = int.Parse(Console.ReadLine());

            switch (opcao)
            {
                case 1:
                    banco.CriarConta();
                    break;
                case 2:
                    banco.Depositar();
                    break;
                case 3:
                    banco.Sacar();
                    break;
                case 4:
                    banco.VerificarSaldo();
                    break;
                case 5:
                    banco.ListarContas();
                    break;
                case 6:
                    Console.WriteLine("Saindo do programa.");
                    return;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }
}

class ContaBancaria
{
    public int Numero { get; }
    public string Titular { get; set; }
    public double Saldo { get; set; }

    public ContaBancaria(int numero, string titular)
    {
        Numero = numero;
        Titular = titular;
        Saldo = 0.0;
    }

    public void Depositar(double valor)
    {
        Saldo += valor;
        Console.WriteLine($"Depósito de {valor:C} realizado com sucesso. Novo saldo: {Saldo:C}");
    }

    public bool Sacar(double valor)
    {
        if (valor > Saldo)
        {
            Console.WriteLine("Saldo insuficiente.");
            return false;
        }

        Saldo -= valor;
        Console.WriteLine($"Saque de {valor:C} realizado com sucesso. Novo saldo: {Saldo:C}");
        return true;
    }

    public override string ToString()
    {
        return $"Conta {Numero} - Titular: {Titular} - Saldo: {Saldo:C}";
    }
}

class Banco
{
    private List<ContaBancaria> contas = new List<ContaBancaria>();
    private string connectionString = "Data Source=BancoDados.db";

    public Banco()
    {
        CriarTabelaContas();
        CarregarContasDoBanco();
    }

    private void CriarTabelaContas()
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string query = "CREATE TABLE IF NOT EXISTS Contas (Numero INTEGER PRIMARY KEY, Titular TEXT, Saldo REAL);";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    private void CarregarContasDoBanco()
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Contas;";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int numero = Convert.ToInt32(reader["Numero"]);
                        string titular = Convert.ToString(reader["Titular"]);
                        double saldo = Convert.ToDouble(reader["Saldo"]);

                        ContaBancaria conta = new ContaBancaria(numero, titular)
                        {
                            Saldo = saldo
                        };

                        contas.Add(conta);
                    }
                }
            }
        }
    }

    private void AtualizarContaNoBanco(ContaBancaria conta)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string query = "UPDATE Contas SET Titular = @titular, Saldo = @saldo WHERE Numero = @numero;";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@titular", conta.Titular);
                command.Parameters.AddWithValue("@saldo", conta.Saldo);
                command.Parameters.AddWithValue("@numero", conta.Numero);

                command.ExecuteNonQuery();
            }
        }
    }

    public void CriarConta()
    {
        Random random = new Random();
        int numeroConta = random.Next(1000, 2000);

        Console.Write("Digite o nome do titular da conta: ");
        string titular = Console.ReadLine();

        ContaBancaria novaConta = new ContaBancaria(numeroConta, titular);
        contas.Add(novaConta);

        // Adiciona a nova conta ao banco de dados
        InserirContaNoBanco(novaConta);

        Console.WriteLine($"Conta criada com sucesso. Número da conta: {numeroConta}");
    }

    private void InserirContaNoBanco(ContaBancaria conta)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string query = "INSERT INTO Contas (Numero, Titular, Saldo) VALUES (@numero, @titular, @saldo);";

            using (SqliteCommand command = new SqliteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@numero", conta.Numero);
                command.Parameters.AddWithValue("@titular", conta.Titular);
                command.Parameters.AddWithValue("@saldo", conta.Saldo);

                command.ExecuteNonQuery();
            }
        }
    }

    public void Depositar()
    {
        Console.Write("Digite o número da conta: ");
        int numeroConta = int.Parse(Console.ReadLine());

        ContaBancaria conta = EncontrarConta(numeroConta);

        if (conta != null)
        {
            Console.Write("Digite o valor a ser depositado: ");
            double valor = double.Parse(Console.ReadLine());

            conta.Depositar(valor);
            AtualizarContaNoBanco(conta);
        }
    }

    public void Sacar()
    {
        Console.Write("Digite o número da conta: ");
        int numeroConta = int.Parse(Console.ReadLine());

        ContaBancaria conta = EncontrarConta(numeroConta);

        if (conta != null)
        {
            Console.Write("Digite o valor a ser sacado: ");
            double valor = double.Parse(Console.ReadLine());

            if (conta.Sacar(valor))
            {
                AtualizarContaNoBanco(conta);
            }
        }
    }

    public void VerificarSaldo()
    {
        Console.Write("Digite o número da conta: ");
        int numeroConta = int.Parse(Console.ReadLine());

        ContaBancaria conta = EncontrarConta(numeroConta);

        if (conta != null)
        {
            Console.WriteLine($"Saldo da conta {numeroConta}: {conta.Saldo:C}");
        }
    }

    public void ListarContas()
    {
        foreach (var conta in contas)
        {
            Console.WriteLine(conta);
        }
    }

    private ContaBancaria EncontrarConta(int numeroConta)
    {
        ContaBancaria conta = contas.FirstOrDefault(c => c.Numero == numeroConta);

        if (conta == null)
        {
            Console.WriteLine("Conta não encontrada. Verifique o número da conta.");
        }

        return conta;
    }
}
