# ModernLauncher - FenrirFS Style Application

## �T�v
FenrirFS���̃N���V�b�N�ȃf�U�C�����������`���[�A�v���P�[�V�����ł��BMVVM�p�^�[����SOLID�����ɏ]���čč\�z����Ă��܂��B

## �A�[�L�e�N�`��

### �t�H���_�\��
```
ModernLauncher/
������ Models/               # �f�[�^���f��
��   ������ Project.cs
��   ������ ProjectInfo.cs
��   ������ ItemGroup.cs
��   ������ LauncherItem.cs
��   ������ SelectableGroup.cs
������ ViewModels/           # �r���[���f���iMVVM�p�^�[���j
��   ������ MainViewModel.cs
������ Views/                # �r���[�iXAML/Window�j
��   ������ MainWindow.xaml
��   ������ MainWindow.xaml.cs
��   ������ TextInputDialog.cs
������ Services/             # �r�W�l�X���W�b�N�E�T�[�r�X
��   ������ ProjectService.cs
��   ������ LauncherService.cs
��   ������ ServiceLocator.cs
������ Interfaces/           # �C���^�[�t�F�[�X��`
��   ������ IProjectService.cs
��   ������ ILauncherService.cs
������ Commands/             # �R�}���h�p�^�[������
��   ������ RelayCommand.cs
������ Converters/           # �f�[�^�o�C���f�B���O�p�R���o�[�^�[
    ������ GroupButtonVisibilityConverter.cs
```

## �݌v����

### MVVM�p�^�[��
- **Model**: �f�[�^�ƃr�W�l�X���W�b�N
- **View**: UI�\���iXAML�j
- **ViewModel**: View��Model�̒���A�f�[�^�o�C���f�B���O

### SOLID�����̓K�p
1. **�P��ӔC�̌��� (SRP)**: �e�N���X��1�̐ӔC�̂�
2. **�J�����̌��� (OCP)**: �g���ɊJ���A�C���ɕ�
3. **���X�R�t�̒u������ (LSP)**: �h���N���X�͊��N���X�̑�։\
4. **�C���^�[�t�F�[�X�����̌��� (ISP)**: �s�v�Ȉˑ��֌W�������
5. **�ˑ��֌W�t�]�̌��� (DIP)**: ���ۂɈˑ��A��ۂɔ�ˑ�

### ���C������
- **Presentation Layer**: Views, ViewModels
- **Business Logic Layer**: Services, Commands
- **Data Access Layer**: Services (ProjectService)
- **Domain Layer**: Models, Interfaces

## ��v�ȉ��P�_

### 1. �ӔC�̕���
- UI�֘A�̃��W�b�N��ViewModel��
- �f�[�^�����Service�N���X��
- �r�W�l�X���[���͓K�؂ȃT�[�r�X�ɕ���

### 2. �ˑ��֌W����
- ServiceLocator�p�^�[���ŃT�[�r�X���Ǘ�
- �C���^�[�t�F�[�X�x�[�X�̐݌v
- �e�X�^�r���e�B�̌���

### 3. �f�[�^�o�C���f�B���O
- View��ViewModel�̊��S����
- INotifyPropertyChanged�̎���
- �R�}���h�p�^�[���ɂ��A�N�V��������

### 4. �G���[�n���h�����O
- �e�w�ł̓K�؂ȗ�O����
- ���[�U�[�t�����h���[�ȃG���[���b�Z�[�W

## �g�p�Z�p
- .NET 6.0
- WPF (Windows Presentation Foundation)
- MVVM (Model-View-ViewModel) �p�^�[��
- Newtonsoft.Json (JSON�V���A���C�[�[�V����)

## ����̊g���\��
1. �A�C�e���ǉ��E�ҏW�_�C�A���O�̎���
2. �C���|�[�g�E�G�N�X�|�[�g�@�\�̊��S����
3. �w���v�V�X�e���̎���
4. �ݒ�Ǘ��@�\
5. �v���O�C���A�[�L�e�N�`��
6. �P�̃e�X�g�̒ǉ�

## �J���Ҍ������

### �V�@�\�̒ǉ����@
1. �K�v�ɉ����ăC���^�[�t�F�[�X���`
2. �T�[�r�X�N���X�Ńr�W�l�X���W�b�N������
3. ViewModel�ɃR�}���h�ƃv���p�e�B��ǉ�
4. View��UI�ƃf�[�^�o�C���f�B���O��ݒ�

### �e�X�g
- �e�T�[�r�X�͓Ɨ����ăe�X�g�\
- ViewModel�̃��W�b�N�͈ˑ��֌W�����Ń��b�N���\
- �r�W�l�X���W�b�N��UI����������Ă��邽�ߒP�̃e�X�g���e��