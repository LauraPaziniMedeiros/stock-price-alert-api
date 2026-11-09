# Stock Price Alert

> 🇺🇸 English | [🇧🇷 Versão em Português](#Alerta-de-Cotação)

## Objective

Develop a C# console application that continuously monitors the price of an
asset traded on B3 and sends email alerts when the price reaches the defined
sell or buy thresholds.

## Requirements

- Monitor an asset specified on the command line.
- Accept three parameters:
  1. The asset symbol.
  2. The reference price for selling.
  3. The reference price for buying.
- Continuously check the price while the program is running.
- Send an email advising the user to sell when the price rises above the sell
  threshold.
- Send an email advising the user to buy when the price falls below the buy
  threshold.
- Read the recipient email address and SMTP server access settings from a
  configuration file.
- Use a price-quote API of the implementer's choice.

## Expected usage

```text
stock-quote-alert.exe PETR4 22.67 22.59
```

In this example, `PETR4` is the asset being monitored, `22.67` is the sell
threshold, and `22.59` is the buy threshold.

The program should send a sell alert when the price is greater than the sell
threshold, and a buy alert when the price is lower than the buy threshold.

## Email configuration

The application should read the following from a configuration file:

- The email address that will receive alerts.
- The SMTP host, port, and credentials required to connect to the server.

The configuration names and file format depend on the implementation. Do not
store real credentials in version control.

# Alerta de cotação

> [🇺🇸 English](#objective) | 🇧🇷 Versão em Português

## Objetivo

Desenvolver uma aplicação de console em C# para monitorar continuamente a
cotação de um ativo negociado na B3 e enviar alertas por e-mail quando o preço
atingir os limites definidos para venda ou compra.

## Requisitos

- Monitorar um ativo informado na linha de comando.
- Receber três parâmetros:
  1. O código do ativo.
  2. O preço de referência para venda.
  3. O preço de referência para compra.
- Consultar continuamente a cotação enquanto o programa estiver em execução.
- Enviar um e-mail aconselhando a venda quando a cotação ficar acima do limite
  de venda.
- Enviar um e-mail aconselhando a compra quando a cotação ficar abaixo do limite
  de compra.
- Ler de um arquivo de configuração o endereço de e-mail de destino e os dados
  de acesso ao servidor SMTP.
- Usar uma API de cotação à escolha da implementação.

## Uso esperado

```text
stock-quote-alert.exe PETR4 22.67 22.59
```

Neste exemplo, `PETR4` é o ativo monitorado, `22.67` é o limite de venda e
`22.59` é o limite de compra.

O programa deve enviar o alerta de venda quando o preço for maior que o limite
de venda e o alerta de compra quando for menor que o limite de compra.

## Configuração de e-mail

A aplicação deverá obter de um arquivo de configuração:

- O e-mail que receberá os alertas.
- O host, a porta e as credenciais necessários para acessar o servidor SMTP.

Os nomes das configurações e o formato do arquivo dependem da implementação.
Não armazene credenciais reais no controle de versão.