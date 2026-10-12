using AgriTrade.Backend.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgriTrade.Backend.Services
{
    public static class DocumentGenerator
    {
        public static QuestPDF.Infrastructure.IDocument GenerateInvoice(Deal deal, List<Commission> commissions, List<Payment> payments)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("AgriTrade - فاکتور فروش")
                                .SemiBold.FontSize(20).FontColor(QuestPDF.Drawing.Colors.Blue.Medium);

                            column.Item().Text($"شناسه معامله: {deal.Id}")
                                .FontSize(10);
                            column.Item().Text($"تاریخ ثبت: {deal.CreatedAt:yyyy/MM/dd HH:mm}")
                                .FontSize(10);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"فروشنده: {deal.Seller?.Name ?? "—"}")
                                .FontSize(11);
                            column.Item().Text($"خریدار: {deal.Buyer?.Name ?? "—"}")
                                .FontSize(11);

                            column.Item().PaddingVertical(5).LineHorizontal(1);

                            column.Item().Text($"محصول: {deal.Product}")
                                .FontSize(11);
                            column.Item().Text($"مقدار (کیلوگرم): {deal.QuantityKg}")
                                .FontSize(11);
                            column.Item().Text($"قیمت واحد (هر کیلو): {deal.UnitPrice:C}")
                                .FontSize(11);
                            column.Item().Text($"ارزش کل: {deal.TotalValue:C}")
                                .FontSize(11);
                            column.Item().Text($"وضعیت: {deal.Status}")
                                .FontSize(11);

                            column.Item().PaddingVertical(5).LineHorizontal(1);

                            if (commissions.Any())
                            {
                                column.Item().Text("کمیسیون‌ها:").SemiBold.FontSize(11);
                                foreach (var c in commissions)
                                {
                                    column.Item().Text($"  نوع: {c.CommissionType} | نرخ: {c.Rate}% | مبلغ: {c.Amount:C} | وضعیت: {c.Status}")
                                        .FontSize(10);
                                }
                            }

                            if (payments.Any())
                            {
                                column.Item().Text("پرداخت‌ها:").SemiBold.FontSize(11);
                                foreach (var p in payments)
                                {
                                    column.Item().Text($"  مبلغ: {p.Amount:C} | روش: {p.Method} | وضعیت: {p.Status} | مرجع: {p.GatewayRef ?? "—"}")
                                        .FontSize(10);
                                }
                            }

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().AlignCenter()
                                .Text("AgriTrade Trading System")
                                .FontSize(10).FontColor(QuestPDF.Drawing.Colors.Grey.Darken2);
                        });

                    page.Header()
                        .Text("سامانه واسطه‌گری انجیر و انار - AgriTrade")
                        .SemiBold.FontSize(14);

                    page.Footer()
                        .AlignCenter()
                        .Text(text => text.CurrentPageNumber().Formatted("صفحه {0} "))
                        .FontSize(9);
                });
            }).PageColor(QuestPDF.Drawing.Colors.Grey.Lighten3);
            return document;
        }

        public static QuestPDF.Infrastructure.IDocument GenerateCommissionReport(Commission commission)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("AgriTrade - گزارش کمیسیون")
                                .SemiBold.FontSize(20).FontColor(QuestPDF.Drawing.Colors.Blue.Medium);

                            column.Item().Text($"شناسه کمیسیون: {commission.Id}")
                                .FontSize(10);
                            column.Item().Text($"تاریخ ثبت: {commission.CreatedAt:yyyy/MM/dd HH:mm}")
                                .FontSize(10);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"شناسه معامله: {commission.DealId}")
                                .FontSize(11);
                            column.Item().Text($"نوع کمیسیون: {commission.CommissionType}")
                                .FontSize(11);
                            column.Item().Text($"نرخ: {commission.Rate}%")
                                .FontSize(11);
                            column.Item().Text($"مبلغ کمیسیون: {commission.Amount:C}")
                                .FontSize(11);
                            column.Item().Text($"وضعیت: {commission.Status}")
                                .FontSize(11);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().AlignCenter()
                                .Text("AgriTrade Trading System")
                                .FontSize(10).FontColor(QuestPDF.Drawing.Colors.Grey.Darken2);
                        });

                    page.Header()
                        .Text("گزارش کمیسیون - سامانه واسطه‌گری انجیر و انار")
                        .SemiBold.FontSize(14);

                    page.Footer()
                        .AlignCenter()
                        .Text(text => text.CurrentPageNumber().Formatted("صفحه {0} "))
                        .FontSize(9);
                });
            });
        }

        public static QuestPDF.Infrastructure.IDocument GenerateDealDocument(Deal deal)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("AgriTrade - سند معامله")
                                .SemiBold.FontSize(20).FontColor(QuestPDF.Drawing.Colors.Blue.Medium);

                            column.Item().Text($"شناسه معامله: {deal.Id}")
                                .FontSize(10);
                            column.Item().Text($"تاریخ ثبت: {deal.CreatedAt:yyyy/MM/dd HH:mm}")
                                .FontSize(10);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"محصول: {deal.Product}")
                                .FontSize(11);
                            column.Item().Text($"مقدار (کیلوگرم): {deal.QuantityKg}")
                                .FontSize(11);
                            column.Item().Text($"قیمت واحد: {deal.UnitPrice:C}")
                                .FontSize(11);
                            column.Item().Text($"ارزش کل: {deal.TotalValue:C}")
                                .FontSize(11);
                            column.Item().Text($"وضعیت: {deal.Status}")
                                .FontSize(11);

                            if (deal.Match?.Offer?.Variety != null)
                            {
                                column.Item().Text($"رقم: {deal.Match.Offer.Variety}")
                                    .FontSize(11);
                            }

                            if (!string.IsNullOrEmpty(deal.Notes))
                            {
                                column.Item().PaddingVertical(5).Text($"یادداشت: {deal.Notes}")
                                    .FontSize(11);
                            }

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().AlignCenter()
                                .Text("AgriTrade Trading System")
                                .FontSize(10).FontColor(QuestPDF.Drawing.Colors.Grey.Darken2);
                        });

                    page.Header()
                        .Text("سند معامله - سامانه واسطه‌گری انجیر و انار")
                        .SemiBold.FontSize(14);

                    page.Footer()
                        .AlignCenter()
                        .Text(text => text.CurrentPageNumber().Formatted("صفحه {0} "))
                        .FontSize(9);
                });
            });
        }

        public static QuestPDF.Infrastructure.IDocument GenerateSellerReport(Seller seller, List<Deal> deals)
        {
            var totalValue = deals.Sum(d => d.TotalValue);
            var totalCount = deals.Count;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("AgriTrade - گزارش فروشنده")
                                .SemiBold.FontSize(20).FontColor(QuestPDF.Drawing.Colors.Blue.Medium);

                            column.Item().Text($"شناسه فروشنده: {seller.Id}")
                                .FontSize(10);
                            column.Item().Text($"تاریخ ثبت: {seller.CreatedAt:yyyy/MM/dd HH:mm}")
                                .FontSize(10);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"نام: {seller.Name}")
                                .FontSize(11);
                            column.Item().Text($"شماره تماس: {seller.Phone}")
                                .FontSize(11);
                            column.Item().Text($"منطقه: {seller.Region ?? "—"}")
                                .FontSize(11);
                            column.Item().Text($"وضعیت: {seller.Status}")
                                .FontSize(11);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"تعداد معاملات: {totalCount}")
                                .FontSize(11);
                            column.Item().Text($"ارزش کل معاملات: {totalValue:C}")
                                .FontSize(11);

                            if (deals.Any())
                            {
                                column.Item().PaddingVertical(5).Text("معاملات:").SemiBold.FontSize(11);
                                foreach (var d in deals)
                                {
                                    column.Item().Text($"  [{d.Id}] {d.Product} - {d.QuantityKg}kg - {d.UnitPrice:C}/kg - {d.TotalValue:C} - {d.Status}")
                                        .FontSize(10);
                                }
                            }

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().AlignCenter()
                                .Text("AgriTrade Trading System")
                                .FontSize(10).FontColor(QuestPDF.Drawing.Colors.Grey.Darken2);
                        });

                    page.Header()
                        .Text("گزارش فروشنده - سامانه واسطه‌گری انجیر و انار")
                        .SemiBold.FontSize(14);

                    page.Footer()
                        .AlignCenter()
                        .Text(text => text.CurrentPageNumber().Formatted("صفحه {0} "))
                        .FontSize(9);
                });
            });
        }

        public static QuestPDF.Infrastructure.IDocument GenerateBuyerReport(Buyer buyer, List<Deal> deals)
        {
            var totalValue = deals.Sum(d => d.TotalValue);
            var totalCount = deals.Count;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text("AgriTrade - گزارش خریدار")
                                .SemiBold.FontSize(20).FontColor(QuestPDF.Drawing.Colors.Blue.Medium);

                            column.Item().Text($"شناسه خریدار: {buyer.Id}")
                                .FontSize(10);
                            column.Item().Text($"تاریخ ثبت: {buyer.CreatedAt:yyyy/MM/dd HH:mm}")
                                .FontSize(10);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"نام: {buyer.Name}")
                                .FontSize(11);
                            column.Item().Text($"شماره تماس: {buyer.Phone}")
                                .FontSize(11);
                            column.Item().Text($"منطقه: {buyer.Region ?? "—"}")
                                .FontSize(11);
                            column.Item().Text($"وضعیت: {buyer.Status}")
                                .FontSize(11);

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().Text($"تعداد معاملات: {totalCount}")
                                .FontSize(11);
                            column.Item().Text($"ارزش کل معاملات: {totalValue:C}")
                                .FontSize(11);

                            if (deals.Any())
                            {
                                column.Item().PaddingVertical(5).Text("معاملات:").SemiBold.FontSize(11);
                                foreach (var d in deals)
                                {
                                    column.Item().Text($"  [{d.Id}] {d.Product} - {d.QuantityKg}kg - {d.UnitPrice:C}/kg - {d.TotalValue:C} - {d.Status}")
                                        .FontSize(10);
                                }
                            }

                            column.Item().PaddingVertical(10).LineHorizontal(1);

                            column.Item().AlignCenter()
                                .Text("AgriTrade Trading System")
                                .FontSize(10).FontColor(QuestPDF.Drawing.Colors.Grey.Darken2);
                        });

                    page.Header()
                        .Text("گزارش خریدار - سامانه واسطه‌گری انجیر و انار")
                        .SemiBold.FontSize(14);

                    page.Footer()
                        .AlignCenter()
                        .Text(text => text.CurrentPageNumber().Formatted("صفحه {0} "))
                        .FontSize(9);
                });
            });
        }
    }
}
