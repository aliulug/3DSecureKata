using AdaGenel.Cesitli;
using AdaPublicGenel.Extensions;
using AdaPublicGenel.Genel;

namespace _3DCekim
{
	public class GuvenliPOS
	{
		private readonly IBankaSecen _bankaSecen;
		private readonly IGuvenliPOSVeritabaniVekili _veritabaniVekili;

		public GuvenliPOS(IBankaSecen bankaSecen, IGuvenliPOSVeritabaniVekili veritabaniVekili)
		{
			_bankaSecen = bankaSecen;
			_veritabaniVekili = veritabaniVekili;
		}

		public IstekSonuc<BankaCekimIstegiSonucu> CekimIstegiGonder(SiparisBilgi siparisBilgi)
		{
			IstekSonuc siparisBilgiOnKontrolSonucu = siparisBilgileriGecerliMi(siparisBilgi);
			if (siparisBilgiOnKontrolSonucu.Basarili.Degil())
				return siparisBilgiOnKontrolSonucu;

			IBanka banka = _bankaSecen.BankaSec(siparisBilgi);

			//string guid = bankayaGonderilecekGuidOlustur();
			string guid = "";

			var cekimIstegiSonuc = banka?.CekimIstegiGonder(siparisBilgi);

			_veritabaniVekili.Bankadan3DOnayiBekleyenIslemKaydet(siparisBilgi, guid, cekimIstegiSonuc.Nesne);

			return cekimIstegiSonuc;
		}

		private string bankayaGonderilecekGuidOlustur()
		{
			return Araclar.RastgeleString(10);
		}

		private IstekSonuc siparisBilgileriGecerliMi(SiparisBilgi siparisBilgi)
		{
			if (siparisBilgi.Kart == null)
				return IstekSonuc.Hata("Sipariş bilgisinde kart bilgileri bulunmalı");

			if (siparisBilgi.Kart.KartNo.Length != 15 && siparisBilgi.Kart.KartNo.Length != 16)
				return IstekSonuc.Hata("Kredi kartı numarası 15 veya 16 hane olmalı");

			if (siparisBilgi.Kart.GuvenlikKodu.Length != 3)
				return IstekSonuc.Hata("Kart güvenlik numarası 3 hane olmalı");


			//todo............

			return IstekSonuc.Basari();

		}
	}
}