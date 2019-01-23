using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdaPublicGenel.Genel;
using NSubstitute;
using NUnit.Framework;

namespace _3DCekim
{
	public class GuvenliPOSTest
	{
		private IBankaSecen _bankaSecen;
		private GuvenliPOS _pos;
		private SiparisBilgi _siparisBilgi;
		private IBanka _banka;
		private string _banka3DOnayUrl;
		private BankaCekimIstegiSonucu _bankaSonuc;
		private IstekSonuc<BankaCekimIstegiSonucu> _bankaninCevabi;
		private IGuvenliPOSVeritabaniVekili _veritabaniVekili;

		[SetUp]
		public void SetUp()
		{
			_bankaSecen = Substitute.For<IBankaSecen>();
			_veritabaniVekili = Substitute.For<IGuvenliPOSVeritabaniVekili>();

			_pos = new GuvenliPOS(_bankaSecen, _veritabaniVekili);
			_siparisBilgi = new SiparisBilgi
			{
				Kart = new KrediKarti
				{
					KartNo = "0123456789123456",
					AdSoyad = "Ali Uluğ",
					GuvenlikKodu = "123",
					SonKullanmaYil = 2020,
					SonKullanmayAy = 1
				},
				Tutar = 100,
				TaksitAdedi = 2
			};
			_banka = Substitute.For<IBanka>();
			_bankaSecen.BankaSec(Arg.Any<SiparisBilgi>()).Returns(_banka);


			_banka3DOnayUrl = "https://garanti.com";
			_bankaSonuc = new BankaCekimIstegiSonucu
			{
				BankaSistemi3DOnayUrl = _banka3DOnayUrl
			};
			_bankaninCevabi = IstekSonuc<BankaCekimIstegiSonucu>.Basari("", _bankaSonuc);
			_banka.CekimIstegiGonder(_siparisBilgi).Returns(_bankaninCevabi);

		}

		[Test]
		public void CekiminHangiBankadanYapilacaginaKararVerilir()
		{
			//when
			_pos.CekimIstegiGonder(_siparisBilgi);

			//then
			_bankaSecen.Received().BankaSec(_siparisBilgi);
		}

		[Test]
		public void BankayaIstekGonderilir()
		{
			//given

			//when
			_pos.CekimIstegiGonder(_siparisBilgi);

			//then
			_banka.Received().CekimIstegiGonder(_siparisBilgi);
		}

		[Test]
		public void SiparisBilgisindeKartNumarasiEksikseHataDonerBankaSecilmez()
		{
			SiparisBilgi gecersizBilgi = new SiparisBilgi
			{
				Kart = new KrediKarti
				{
					KartNo = "",
					AdSoyad = "Ali Uluğ",
					GuvenlikKodu = "123",
					SonKullanmaYil = 2020,
					SonKullanmayAy = 1
				},
				Tutar = 100,
				TaksitAdedi = 2
			};

			IstekSonuc<BankaCekimIstegiSonucu> sonuc = _pos.CekimIstegiGonder(gecersizBilgi);

			Assert.IsFalse(sonuc.Basarili);
			_bankaSecen.DidNotReceive().BankaSec(Arg.Any<SiparisBilgi>());
		}

		[Test]
		public void BankadanDonen3DOnayUrlIstemciyeDoner()
		{

			IstekSonuc<BankaCekimIstegiSonucu> sonuc = _pos.CekimIstegiGonder(_siparisBilgi);

			Assert.AreEqual(sonuc.Nesne.BankaSistemi3DOnayUrl, _banka3DOnayUrl);
		}

		[Test]
		public void BankayaCekimIstegiGonderildigindeAsenkronCevaptaKullanilmakUzereSiparisBilgisiKaydedilir()
		{
			_pos.CekimIstegiGonder(_siparisBilgi);

			_veritabaniVekili.ReceivedWithAnyArgs().Bankadan3DOnayiBekleyenIslemKaydet(_siparisBilgi, "", _bankaSonuc);
		}
	}

	public interface IGuvenliPOSVeritabaniVekili
	{
		void Bankadan3DOnayiBekleyenIslemKaydet(SiparisBilgi siparisBilgi, string guid, BankaCekimIstegiSonucu bankaSonuc);
	}

	public interface IBankaSecen
	{
		IBanka BankaSec(SiparisBilgi siparisBilgi);
	}

	public interface IBanka
	{
		IstekSonuc<BankaCekimIstegiSonucu> CekimIstegiGonder(SiparisBilgi siparisBilgi);
	}

	public class BankaCekimIstegiSonucu
	{
		public string BankaSistemi3DOnayUrl;
	}

	public class SiparisBilgi
	{
		public KrediKarti Kart;
		public double Tutar;
		public int TaksitAdedi;

		public string SiparisAnahtar;//poliçe zeyil key olabilir veya kullanım amacına göre değişir
		public string PassUrl;
		public string FailUrl;


	}

	public class KrediKarti
	{
		public string KartNo;
		public string AdSoyad;
		public string GuvenlikKodu;
		public int SonKullanmayAy;
		public int SonKullanmaYil;

	}
}
